using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using VigorSNMP.Model;

namespace VigorSNMP.Services
{
    public class VigorSnmpService
    {
        private readonly IPEndPoint endPoint;
        private readonly OctetString community;
        private readonly VersionCode version;
        private readonly int timeoutMs;
        private readonly SnmpV3Credentials? v3Credentials;
     
        public VigorSnmpService(string host, string communityString = "public", int port = 161, VersionCode snmpVersion = VersionCode.V2, int timeoutMs = 5000)
        {
            if (snmpVersion == VersionCode.V3)
                throw new ArgumentException("Use the V3 constructor overload that accepts SnmpV3Credentials.", nameof(snmpVersion));

            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            community = new OctetString(communityString);
            version = snmpVersion;
            this.timeoutMs = timeoutMs;
            v3Credentials = null;
        }

        public VigorSnmpService(string host, SnmpV3Credentials credentials, int port = 161, int timeoutMs = 5000)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            community = OctetString.Empty;
            version = VersionCode.V3;
            this.timeoutMs = timeoutMs;
            v3Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        }

        public async Task<SystemDeviceInfo> GetSystemInfoAsync(CancellationToken ct = default)
        {
            return BuildSystemInfo(await SendGetAsync(SystemOids(), ct));
        }

        public async Task<Vdsl2LineStatusInfo> GetVdsl2LineStatusAsync(int dslIfIndex, CancellationToken ct = default)
        {
            return BuildVdsl2LineStatus(await SendGetAsync(Vdsl2LineOids(dslIfIndex), ct), dslIfIndex);
        }

        public async Task<Vdsl2ChannelInfo> GetVdsl2ChannelAsync(int dslIfIndex, CancellationToken ct = default)
        {
            return BuildVdsl2Channel(await SendGetAsync(Vdsl2ChannelOids(dslIfIndex), ct), dslIfIndex);
        }

        public async Task<Vdsl2Performance15MinInfo> GetVdsl2PerformanceAsync(int dslIfIndex, CancellationToken ct = default)
        {
            return BuildVdsl2Performance(await SendGetAsync(Vdsl2PerfOids(dslIfIndex), ct), dslIfIndex);
        }

        public async Task<Vdsl2LineStatusInfo> GetAdslLineStatusAsync(CancellationToken ct = default)
        {
            return BuildAdslLineStatus(await SendGetAsync(AdslLineStatusOids(), ct));
        }

        public async Task<Vdsl2ChannelInfo> GetAdslChannelAsync(CancellationToken ct = default)
        {
            return BuildAdslChannel(await SendGetAsync(AdslChannelOids(), ct));
        }

        public async Task<LanInterfaceInfo> GetLanInterfaceAsync(int ifIndex = 1, CancellationToken ct = default)
        {
            return BuildLanInterface(await SendGetAsync(LanOids(ifIndex), ct), ifIndex);
        }

        public async Task<int> DiscoverVdsl2IfIndexAsync(CancellationToken ct = default)
        {
            // Walk the xdsl2LineActTransSys column — first returned OID suffix IS the ifIndex
            string actTransSysBase = "1.3.6.1.2.1.10.251.1.2.1.1.1";

            IList<Variable> walked = await Task.Run(() =>
            {
                IList<Variable> results = new List<Variable>();
                Messenger.Walk(
                    this.version,
                    this.endPoint,
                    this.community,
                    new ObjectIdentifier(actTransSysBase),
                    results,
                    this.timeoutMs,
                    WalkMode.WithinSubtree);
                return results;
            }, ct);

            foreach (Variable v in walked)
            {
                // OID = 1.3.6.1.2.1.10.251.1.2.1.1.1.{ifIndex}
                string oid = v.Id.ToString();
                if (oid.StartsWith(actTransSysBase + "."))
                {
                    string suffix = oid.Substring(actTransSysBase.Length + 1);
                    if (int.TryParse(suffix, out int idx))
                        return idx;
                }
            }

            return -1; // VDSL2-LINE-MIB not populated — device may use private MIB
        }

        public async Task<Dictionary<int, string>> GetInterfaceMapAsync(CancellationToken ct = default)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();

            IList<Variable> walked = await Task.Run(() =>
            {
                IList<Variable> results = new List<Variable>();
                Messenger.Walk(
                    this.version,
                    this.endPoint,
                    this.community,
                    new ObjectIdentifier("1.3.6.1.2.1.2.2.1.2"),
                    results,
                    this.timeoutMs,
                    WalkMode.WithinSubtree);
                return results;
            }, ct);

            foreach (Variable v in walked)
            {
                string[] parts = v.Id.ToString().Split('.');
                if (int.TryParse(parts[parts.Length - 1], out int idx))
                    map[idx] = v.Data.ToString() ?? string.Empty;
            }

            return map;
        }

        public async Task<int> FindLanBridgeIfIndexAsync(CancellationToken ct = default)
        {
            Dictionary<int, string> map = await GetInterfaceMapAsync(ct);

            string[] lanNames = new[] { "br0", "br-lan", "eth0", "eth1", "vlan1" };

            foreach (string name in lanNames)
            {
                foreach (KeyValuePair<int, string> kvp in map)
                {
                    if (string.Equals(kvp.Value, name, StringComparison.OrdinalIgnoreCase))
                        return kvp.Key;
                }
            }

            return 1;
        }

        public string DiagnoseV3Connection()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"  Host         : {endPoint}");
            sb.AppendLine($"  Version      : {version}");
            sb.AppendLine($"  Username     : {v3Credentials?.Username ?? "n/a"}");
            sb.AppendLine($"  Auth protocol: {v3Credentials?.AuthProtocol}");
            sb.AppendLine($"  Priv protocol: {v3Credentials?.PrivProtocol}");
            sb.AppendLine($"  Timeout      : {timeoutMs} ms");
            sb.AppendLine();

            // ── Phase 1: basic UDP reachability via V2c ping ──────────────────────────
            sb.Append("  [1] V2c reachability (sysDescr)... ");
            try
            {
                IList<Variable> ping = Messenger.Get(VersionCode.V2,
                    endPoint,
                    new OctetString("public"),
                    new List<Variable> { new Variable(new ObjectIdentifier(Vigor167Oids.SysDescr)) },
                    timeoutMs);
                sb.AppendLine($"OK — {ping[0].Data}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"FAILED — {ex.GetType().Name}: {ex.Message}");
                sb.AppendLine("  ► Device is not reachable on UDP 161. Check IP, firewall, and SNMP enabled.");
                return sb.ToString();
            }

            // ── Phase 2: V3 discovery ─────────────────────────────────────────────────
            sb.Append("  [2] V3 discovery (engine ID)... ");
            ReportMessage report;
            try
            {
                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                report = discovery.GetResponse(timeoutMs, endPoint);
                sb.AppendLine($"OK — engine ID: {BitConverter.ToString(report.Parameters.EngineId?.GetRaw() ?? [])}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"FAILED — {ex.GetType().Name}: {ex.Message}");
                sb.AppendLine("  ► V3 is not enabled on the device, or the device is blocking V3 discovery.");
                sb.AppendLine("  ► Check: System Maintenance → SNMP → enable SNMPv3.");
                return sb.ToString();
            }

            // ── Phase 3: V3 authenticated GET ────────────────────────────────────────
            sb.Append("  [3] V3 authenticated GET (sysDescr)... ");
            try
            {
                IPrivacyProvider privacy = BuildPrivacyProvider();
                GetRequestMessage request = new GetRequestMessage(
                    VersionCode.V3,
                    Messenger.NextMessageId,
                    Messenger.NextRequestId,
                    new OctetString(v3Credentials?.Username ?? string.Empty),
                    OctetString.Empty,
                    new List<Variable> { new Variable(new ObjectIdentifier(Vigor167Oids.SysDescr)) },
                    privacy,
                    Messenger.MaxMessageSize,
                    report);

                ISnmpMessage response = request.GetResponse(timeoutMs, endPoint);

                if (response is ReportMessage reportMsg)
                {
                    IList<Variable> vars = reportMsg.Pdu().Variables;
                    string errorOid = vars.Count > 0 ? vars[0].Id.ToString() : "unknown";
                    sb.AppendLine($"REPORT received — OID: {errorOid}");
                    sb.AppendLine(DecodeReportOid(errorOid));
                }
                else
                {
                    sb.AppendLine($"OK — {response.Pdu().Variables[0].Data}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"FAILED — {ex.GetType().Name}: {ex.Message}");
                sb.AppendLine("  ► Credentials mismatch — verify username, auth password, auth protocol,");
                sb.AppendLine("    priv password and priv protocol against the router's SNMP user settings.");
            }

            return sb.ToString();
        }

        public string DiagnoseV3Combinations()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            ReportMessage report;

            try
            {
                report = discovery.GetResponse(timeoutMs, endPoint);
                sb.AppendLine($"  Discovery OK — engine: {BitConverter.ToString(report.Parameters.EngineId?.GetRaw() ?? [])}");
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  Discovery FAILED: {ex.Message}");
                return sb.ToString();
            }

            SnmpV3AuthProtocol[] authProtocols = new[]
            {
                SnmpV3AuthProtocol.MD5,
                SnmpV3AuthProtocol.SHA1
            };

            SnmpV3PrivProtocol[] privProtocols = new[]
            {
                SnmpV3PrivProtocol.None,
                SnmpV3PrivProtocol.DES,
                SnmpV3PrivProtocol.AES128,
                SnmpV3PrivProtocol.AES192,
                SnmpV3PrivProtocol.AES256
            };

            foreach (SnmpV3AuthProtocol auth in authProtocols)
            {
                foreach (SnmpV3PrivProtocol priv in privProtocols)
                {
                    string combo = $"auth={auth,-6} priv={priv,-7}";
                    sb.Append($"  Testing {combo} ... ");

                    try
                    {
                        IAuthenticationProvider authProvider = BuildAuthProvider(auth, v3Credentials?.AuthPassword ?? string.Empty);
                        IPrivacyProvider privProvider = BuildPrivProvider(priv, v3Credentials?.PrivPassword ?? string.Empty, authProvider);

                        GetRequestMessage request = new GetRequestMessage(
                            VersionCode.V3,
                            Messenger.NextMessageId,
                            Messenger.NextRequestId,
                            new OctetString(v3Credentials?.Username ?? string.Empty),
                            OctetString.Empty,
                            new List<Variable> { new Variable(new ObjectIdentifier(Vigor167Oids.SysDescr)) },
                            privProvider,
                            Messenger.MaxMessageSize,
                            report);

                        ISnmpMessage response = request.GetResponse(timeoutMs, endPoint);

                        if (response is ReportMessage reportMsg)
                        {
                            IList<Variable> vars = reportMsg.Pdu().Variables;
                            string errorOid = vars.Count > 0 ? vars[0].Id.ToString() : "unknown";
                            sb.AppendLine($"REPORT — {DecodeReportOid(errorOid).Trim()}");
                        }
                        else if (response.Pdu().ErrorStatus.ToInt32() == 0)
                        {
                            sb.AppendLine($"✓ SUCCESS ← use this combination");
                        }
                        else
                        {
                            sb.AppendLine($"PDU error {response.Pdu().ErrorStatus}");
                        }
                    }
                    catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                    {
                        sb.AppendLine("TIMEOUT — priv protocol not supported by device");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"ERROR — {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }

            return sb.ToString();
        }

        public async Task<string> DiagnosePrivateMibAsync(CancellationToken ct = default)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Candidates in priority order:
            // RFC 5650  — already confirmed absent (DiscoverVdsl2IfIndexAsync returned -1)
            // RFC 2662  — older ADSL-LINE-MIB, still used by many DSL routers
            // RFC 3728  — VDSL-LINE-MIB (pre-VDSL2 era)
            // DrayTek   — private enterprise subtrees
            string[] subtrees = new[]
            {
                "1.3.6.1.2.1.10.251.1.1.2",
                "1.3.6.1.4.1.7367",
                "1.3.6.1.2.1.10.94.1.1",    // RFC 2662 adslLineTable
                "1.3.6.1.2.1.10.94.1.2",    // RFC 2662 adslAtucPhysTable (line stats)
                "1.3.6.1.2.1.10.94.1.3",    // RFC 2662 adslAturPhysTable (remote stats)
                "1.3.6.1.2.1.10.94.1.8",    // RFC 2662 adslAtucPerfDataTable
                "1.3.6.1.2.1.10.98",        // RFC 3728 VDSL-LINE-MIB root
                "1.3.6.1.4.1.7367.1",       // DrayTek private subtree 1
                "1.3.6.1.4.1.7367.2",       // DrayTek private subtree 2
                "1.3.6.1.4.1.7367.3",       // DrayTek device info (confirmed working)
            };

            foreach (string subtree in subtrees)
            {
                IList<Variable> results = await Task.Run(() =>
                {
                    IList<Variable> walked = new List<Variable>();
                    try
                    {
                        Messenger.Walk(version, endPoint, community,
                            new ObjectIdentifier(subtree), walked, timeoutMs,
                            WalkMode.WithinSubtree);
                    }
                    catch (Exception ex)
                    {
                        walked.Add(new Variable(new ObjectIdentifier(subtree),
                            new OctetString("WALK ERROR: " + ex.Message)));
                    }
                    return walked;
                }, ct);

                sb.AppendLine($" ── {subtree}  ({results.Count} objects) ──────────────────────────");
                foreach (Variable v in results)
                    sb.AppendLine($"   {v.Id,-60}  {v.Data.TypeCode,-18} {v.Data}");
            }

            return sb.ToString();
        }

        private static string[] SystemOids() => new[]
        {
            Vigor167Oids.SysDescr,
            Vigor167Oids.SysUpTime,
            Vigor167Oids.SysContact,
            Vigor167Oids.SysName,
            Vigor167Oids.SysLocation,
            Vigor167Oids.RouterModel,
            Vigor167Oids.RouterRevision,
            Vigor167Oids.FwBuildDate,
            Vigor167Oids.DslChipVersion,
            Vigor167Oids.MemoryUsage,
            Vigor167Oids.LanMac
        };

        private static string[] Vdsl2LineOids(int dslIfIndex) => new[]
        {
            Vigor167Oids.Vdsl2LineActTransSys  + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineActProfile + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineActMode + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LinePwrMngState + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineLastStateDs + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineLastStateUs + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineCoDefects + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineCpeDefects + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineInitResult + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineSnrMgnDs + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineSnrMgnUs + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineAttenuationDs + "." + dslIfIndex,
            Vigor167Oids.Vdsl2LineAttenuationUs + "." + dslIfIndex,
        };

        private static string[] Vdsl2ChannelOids(int dslIfIndex) => new[]
        {
            Vigor167Oids.Vdsl2ChActDataRate  + "." + dslIfIndex + ".1",  // DS rate
            Vigor167Oids.Vdsl2ChActDataRate  + "." + dslIfIndex + ".2",  // US rate
            Vigor167Oids.Vdsl2ChPrevDataRate + "." + dslIfIndex + ".1",  // prev DS rate
            Vigor167Oids.Vdsl2ChPrevDataRate + "." + dslIfIndex + ".2",  // prev US rate
            Vigor167Oids.Vdsl2ChActInp      + "." + dslIfIndex + ".1",  // INP DS
            Vigor167Oids.Vdsl2ChActInp      + "." + dslIfIndex + ".2",  // INP US
            Vigor167Oids.Vdsl2ChActDelay    + "." + dslIfIndex + ".1",  // interleave delay
        };

        private static string[] Vdsl2PerfOids(int dslIfIndex) => new[]
         {
            Vigor167Oids.Vdsl2Pm15MinEs   + "." + dslIfIndex + ".1",  // CO  errored secs
            Vigor167Oids.Vdsl2Pm15MinEs   + "." + dslIfIndex + ".2",  // CPE errored secs
            Vigor167Oids.Vdsl2Pm15MinSes  + "." + dslIfIndex + ".1",  // CO  severely errored secs
            Vigor167Oids.Vdsl2Pm15MinSes  + "." + dslIfIndex + ".2",  // CPE severely errored secs
            Vigor167Oids.Vdsl2Pm15MinLoss + "." + dslIfIndex + ".1",  // CO  loss-of-signal secs
            Vigor167Oids.Vdsl2Pm15MinLoss + "." + dslIfIndex + ".2",  // CPE loss-of-signal secs
            Vigor167Oids.Vdsl2Pm15MinUas  + "." + dslIfIndex + ".1",  // CO  unavailable secs
            Vigor167Oids.Vdsl2Pm15MinUas  + "." + dslIfIndex + ".2",  // CPE unavailable secs
        };

        private static string[] AdslLineStatusOids() => new[]
        {
            Vigor167Oids.AdslLineConfProfile,
            Vigor167Oids.AdslAtucSnrMgn, Vigor167Oids.AdslAtucAtn,
            Vigor167Oids.AdslAtucStatus, Vigor167Oids.AdslAtucOutputPwr,
            Vigor167Oids.AdslAtucAttainRate,
            Vigor167Oids.AdslAturSnrMgn, Vigor167Oids.AdslAturAtn,
            Vigor167Oids.AdslAturStatus, Vigor167Oids.AdslAturOutputPwr,
            Vigor167Oids.AdslAturAttainRate, Vigor167Oids.AdslAturChipVersion,
        };

        private static string[] AdslChannelOids() => new[]
        {
            Vigor167Oids.AdslAtucChanDelay, Vigor167Oids.AdslAtucChanTxRate,
            Vigor167Oids.AdslAtucChanPrevRate,
            Vigor167Oids.AdslAturChanDelay, Vigor167Oids.AdslAturChanTxRate,
            Vigor167Oids.AdslAturChanPrevRate,
        };

        private static string[] LanOids(int ifIndex) => new[]
        {
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColDescr, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColSpeed, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOperStatus, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColInOctets, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColInDiscards, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColInErrors, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOutOctets, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOutDiscards, ifIndex),
            Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOutErrors, ifIndex)
        };

        private static SystemDeviceInfo BuildSystemInfo(IList<Variable> vars)
        {
            SystemDeviceInfo info = new SystemDeviceInfo();
            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();

                if (oid == Vigor167Oids.SysDescr)
                    info.SystemDescription = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.SysUpTime && v.Data is TimeTicks ticks)
                    info.SystemUpTime = TimeSpan.FromMilliseconds(ticks.ToUInt32() * 10L);
                else if (oid == Vigor167Oids.SysContact)
                    info.SystemContact = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.SysName)
                    info.SystemName = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.SysLocation)
                    info.SystemLocation = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.RouterModel)
                    info.RouterModel = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.RouterRevision)
                    info.FirmwareRevision = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.FwBuildDate)
                    info.FirmwareBuildDate = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.DslChipVersion)
                    info.DslChipsetVersion = v.Data.ToString() ?? string.Empty;
                else if (oid == Vigor167Oids.MemoryUsage
                      && int.TryParse(v.Data.ToString(), out int mem))
                    info.MemoryUsagePercent = mem;
                else if (oid == Vigor167Oids.LanMac)
                    info.LanMacAddress = FormatMac(v.Data);
            }
            return info;
        }

        private static Vdsl2LineStatusInfo BuildVdsl2LineStatus(IList<Variable> vars, int dslIfIndex)
        {
            Vdsl2LineStatusInfo info = new Vdsl2LineStatusInfo();
            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();
                bool hasInt = int.TryParse(v.Data.ToString(), out int raw);

                if (oid == Vigor167Oids.Vdsl2LineActTransSys + "." + dslIfIndex && hasInt) info.ActTransmissionSystem = raw;
                else if (oid == Vigor167Oids.Vdsl2LineActProfile + "." + dslIfIndex && hasInt) info.ActProfile = raw;
                else if (oid == Vigor167Oids.Vdsl2LineActMode + "." + dslIfIndex && hasInt) info.ActMode = raw;
                else if (oid == Vigor167Oids.Vdsl2LinePwrMngState + "." + dslIfIndex && hasInt) info.PowerManagementState = raw;
                else if (oid == Vigor167Oids.Vdsl2LineLastStateDs + "." + dslIfIndex && hasInt) info.LastStateDs = raw;
                else if (oid == Vigor167Oids.Vdsl2LineLastStateUs + "." + dslIfIndex && hasInt) info.LastStateUs = raw;
                else if (oid == Vigor167Oids.Vdsl2LineCoDefects + "." + dslIfIndex && hasInt) info.CoDefects = raw;
                else if (oid == Vigor167Oids.Vdsl2LineCpeDefects + "." + dslIfIndex && hasInt) info.CpeDefects = raw;
                else if (oid == Vigor167Oids.Vdsl2LineInitResult + "." + dslIfIndex && hasInt) info.InitResult = raw;
                else if (oid == Vigor167Oids.Vdsl2LineSnrMgnDs + "." + dslIfIndex && hasInt) info.SnrMarginDsDb = raw / 10.0;
                else if (oid == Vigor167Oids.Vdsl2LineSnrMgnUs + "." + dslIfIndex && hasInt) info.SnrMarginUsDb = raw / 10.0;
                else if (oid == Vigor167Oids.Vdsl2LineAttenuationDs + "." + dslIfIndex && hasInt) info.AttenuationDsDb = raw / 10.0;
                else if (oid == Vigor167Oids.Vdsl2LineAttenuationUs + "." + dslIfIndex && hasInt) info.AttenuationUsDb = raw / 10.0;
            }
            return info;
        }

        private static Vdsl2ChannelInfo BuildVdsl2Channel(IList<Variable> vars, int dslIfIndex)
        {
            Vdsl2ChannelInfo info = new Vdsl2ChannelInfo();
            string dsRate = Vigor167Oids.Vdsl2ChActDataRate + "." + dslIfIndex + ".1";
            string usRate = Vigor167Oids.Vdsl2ChActDataRate + "." + dslIfIndex + ".2";
            string dsPrev = Vigor167Oids.Vdsl2ChPrevDataRate + "." + dslIfIndex + ".1";
            string usPrev = Vigor167Oids.Vdsl2ChPrevDataRate + "." + dslIfIndex + ".2";
            string dsInp = Vigor167Oids.Vdsl2ChActInp + "." + dslIfIndex + ".1";
            string usInp = Vigor167Oids.Vdsl2ChActInp + "." + dslIfIndex + ".2";
            string delay = Vigor167Oids.Vdsl2ChActDelay + "." + dslIfIndex + ".1";

            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();
                if (!long.TryParse(v.Data.ToString(), out long raw)) continue;

                if (oid == dsRate) info.CurrentDsRateBps = raw;
                else if (oid == usRate) info.CurrentUsRateBps = raw;
                else if (oid == dsPrev) info.PrevDsRateBps = raw;
                else if (oid == usPrev) info.PrevUsRateBps = raw;
                else if (oid == dsInp) info.ImpulseNoiseProtDs = raw / 10.0;
                else if (oid == usInp) info.ImpulseNoiseProtUs = raw / 10.0;
                else if (oid == delay) info.InterleaveDelayMs = (int)raw;
            }
            return info;
        }

        private static Vdsl2Performance15MinInfo BuildVdsl2Performance(IList<Variable> vars, int dslIfIndex)
        {
            Vdsl2Performance15MinInfo info = new Vdsl2Performance15MinInfo();
            string coEs = Vigor167Oids.Vdsl2Pm15MinEs + "." + dslIfIndex + ".1";
            string cpeEs = Vigor167Oids.Vdsl2Pm15MinEs + "." + dslIfIndex + ".2";
            string coSes = Vigor167Oids.Vdsl2Pm15MinSes + "." + dslIfIndex + ".1";
            string cpeSes = Vigor167Oids.Vdsl2Pm15MinSes + "." + dslIfIndex + ".2";
            string coLoss = Vigor167Oids.Vdsl2Pm15MinLoss + "." + dslIfIndex + ".1";
            string cpeLoss = Vigor167Oids.Vdsl2Pm15MinLoss + "." + dslIfIndex + ".2";
            string coUas = Vigor167Oids.Vdsl2Pm15MinUas + "." + dslIfIndex + ".1";
            string cpeUas = Vigor167Oids.Vdsl2Pm15MinUas + "." + dslIfIndex + ".2";

            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();
                if (!long.TryParse(v.Data.ToString(), out long raw)) continue;

                if (oid == coEs) info.CoErroredSecs = raw;
                else if (oid == cpeEs) info.CpeErroredSecs = raw;
                else if (oid == coSes) info.CoSeverelyErroredSecs = raw;
                else if (oid == cpeSes) info.CpeSeverelyErroredSecs = raw;
                else if (oid == coLoss) info.CoLossOfSignalSecs = raw;
                else if (oid == cpeLoss) info.CpeLossOfSignalSecs = raw;
                else if (oid == coUas) info.CoUnavailableSecs = raw;
                else if (oid == cpeUas) info.CpeUnavailableSecs = raw;
            }
            return info;
        }

        private static string DecodeReportOid(string oid) => oid switch
        {
            "1.3.6.1.6.3.15.1.1.1.0" => "  ► usmStatsUnsupportedSecLevels — security level not supported by device.",
            "1.3.6.1.6.3.15.1.1.2.0" => "  ► usmStatsNotInTimeWindows    — time window mismatch; will auto-retry.",
            "1.3.6.1.6.3.15.1.1.3.0" => "  ► usmStatsUnknownUserNames    — username not found on device.",
            "1.3.6.1.6.3.15.1.1.4.0" => "  ► usmStatsUnknownEngineIDs    — engine ID mismatch.",
            "1.3.6.1.6.3.15.1.1.5.0" => "  ► usmStatsWrongDigests        — auth password is wrong.",
            "1.3.6.1.6.3.15.1.1.6.0" => "  ► usmStatsDecryptionErrors    — priv password or protocol is wrong.",
            _ => $"  ► Unknown REPORT OID: {oid}"
        };

        private static Vdsl2LineStatusInfo BuildAdslLineStatus(IList<Variable> vars)
        {
            Vdsl2LineStatusInfo info = new Vdsl2LineStatusInfo();
            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();
                string rawStr = v.Data.ToString() ?? string.Empty;
                bool hasLong = long.TryParse(rawStr, out long num);

                if (oid == Vigor167Oids.AdslAtucSnrMgn && hasLong) info.SnrMarginDsDb = num;
                else if (oid == Vigor167Oids.AdslAturSnrMgn && hasLong) info.SnrMarginUsDb = num;
                else if (oid == Vigor167Oids.AdslAtucAtn && hasLong) info.AttenuationDsDb = num;
                else if (oid == Vigor167Oids.AdslAturAtn && hasLong) info.AttenuationUsDb = num;
                else if (oid == Vigor167Oids.AdslAtucOutputPwr && hasLong) info.CoDsOutputPowerDbm = num / 10.0;
                else if (oid == Vigor167Oids.AdslAturOutputPwr && hasLong) info.CpeUsOutputPowerDbm = num / 10.0;
                else if (oid == Vigor167Oids.AdslAtucAttainRate && hasLong) info.AttainableDsRateBps = num;
                else if (oid == Vigor167Oids.AdslAturAttainRate && hasLong) info.AttainableUsRateBps = num;
                else if (oid == Vigor167Oids.AdslAtucStatus) info.LineStatusCo = rawStr;
                else if (oid == Vigor167Oids.AdslAturStatus) info.LineStatusCpe = rawStr;
                else if (oid == Vigor167Oids.AdslLineConfProfile) info.AdslRawProfileName = rawStr;
            }
            return info;
        }

        private static Vdsl2ChannelInfo BuildAdslChannel(IList<Variable> vars)
        {
            Vdsl2ChannelInfo info = new Vdsl2ChannelInfo();
            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();
                if (!long.TryParse(v.Data.ToString(), out long raw)) continue;

                if (oid == Vigor167Oids.AdslAtucChanTxRate) info.CurrentDsRateBps = raw;
                else if (oid == Vigor167Oids.AdslAturChanTxRate) info.CurrentUsRateBps = raw;
                else if (oid == Vigor167Oids.AdslAtucChanPrevRate) info.PrevDsRateBps = raw;
                else if (oid == Vigor167Oids.AdslAturChanPrevRate) info.PrevUsRateBps = raw;
                else if (oid == Vigor167Oids.AdslAtucChanDelay) info.InterleaveDelayMs = (int)raw;
            }
            return info;
        }

        private static LanInterfaceInfo BuildLanInterface(IList<Variable> vars, int ifIndex)
        {
            LanInterfaceInfo info = new LanInterfaceInfo { IfIndex = ifIndex };
            foreach (Variable v in vars)
            {
                if (IsNoSuchInstance(v.Data)) continue;
                string oid = v.Id.ToString();

                string descrOid = Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColDescr, ifIndex);
                if (oid == descrOid)
                {
                    info.Description = v.Data.ToString() ?? string.Empty;
                    continue;
                }
                
                if (!long.TryParse(v.Data.ToString(), out long raw)) continue;

                if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColSpeed, ifIndex)) info.SpeedBps = raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOperStatus, ifIndex)) info.OperStatus = (int)raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColInOctets, ifIndex)) info.InOctets = raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColInDiscards, ifIndex)) info.InDiscards = raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColInErrors, ifIndex)) info.InErrors = raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOutOctets, ifIndex)) info.OutOctets = raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOutDiscards, ifIndex)) info.OutDiscards = raw;
                else if (oid == Vigor167Oids.GetLanIfOid(Vigor167Oids.IfColOutErrors, ifIndex)) info.OutErrors = raw;
            }
            return info;
        }

        private Task<IList<Variable>> SendGetAsync(IList<string> oids, CancellationToken ct)
        {
            IList<string> oidList = new List<string>(oids);
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                IList<Variable> result = version == VersionCode.V1
                    ? SendGetV1OneByOne(oidList)
                    : version == VersionCode.V3
                        ? SendGetV3(oidList)
                        : new List<Variable>(Messenger.Get(version, endPoint, community, BuildVariables(oidList), timeoutMs));

                ct.ThrowIfCancellationRequested();
                return result;
            }, ct);
        }

        private IList<Variable> SendGetV1OneByOne(IList<string> oids)
        {
            List<Variable> results = new List<Variable>(oids.Count);
            foreach (string oid in oids)
            {
                try
                {
                    IList<Variable> single = BuildVariables(new[] { oid });
                    IList<Variable> response = Messenger.Get(VersionCode.V1, endPoint, community, single, timeoutMs);
                    results.AddRange(response);
                }
                catch
                {
                }
            }
            return results;
        }

        private List<Variable> SendGetV3(IList<string> oids)
        {
            IPrivacyProvider privacy = BuildPrivacyProvider();

            // Step 1: Discovery — Discovery has its own GetResponse, it is NOT ISnmpMessage
            Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            ReportMessage report = discovery.GetResponse(timeoutMs, endPoint);

            // Step 2: First authenticated GET
            GetRequestMessage request = BuildV3Request(oids, privacy, report);
            ISnmpMessage response = request.GetResponse(timeoutMs, endPoint);

            // Step 3: RFC 3414 time-window re-sync — agent may reject first attempt
            if (response is ReportMessage)
            {
                ObjectIdentifier notInTimeWindow = new ObjectIdentifier("1.3.6.1.6.3.15.1.1.2.0");
                IList<Variable> reportVars = response.Pdu().Variables;

                bool isTimeWindowError = reportVars.Count > 0
                    && reportVars[0].Id == notInTimeWindow;

                if (!isTimeWindowError)
                    throw ErrorException.Create(
                        "SNMP V3 unexpected REPORT — not a time-window error.",
                        endPoint.Address,
                        response);

                GetRequestMessage retryRequest = BuildV3Request(oids, privacy, response);
                response = retryRequest.GetResponse(timeoutMs, endPoint);
            }

            if (response.Pdu().ErrorStatus.ToInt32() != 0)
                throw ErrorException.Create("error in response", endPoint.Address, response);

            return new List<Variable>(response.Pdu().Variables);
        }

        private IAuthenticationProvider BuildAuthProvider(SnmpV3AuthProtocol auth, string password)
        {
            OctetString pwd = new OctetString(password);
            return auth switch
            {
                SnmpV3AuthProtocol.MD5 => new MD5AuthenticationProvider(pwd),
                SnmpV3AuthProtocol.SHA1 => new SHA1AuthenticationProvider(pwd),
                SnmpV3AuthProtocol.SHA256 => new SHA256AuthenticationProvider(pwd),
                SnmpV3AuthProtocol.SHA384 => new SHA384AuthenticationProvider(pwd),
                SnmpV3AuthProtocol.SHA512 => new SHA512AuthenticationProvider(pwd),
                _ => throw new NotSupportedException($"Auth protocol {auth} not supported.")
            };
        }

        private IPrivacyProvider BuildPrivProvider(SnmpV3PrivProtocol priv, string password, IAuthenticationProvider auth)
        {
            OctetString pwd = new OctetString(password);
            return priv switch
            {
                SnmpV3PrivProtocol.None => new DefaultPrivacyProvider(auth),
                SnmpV3PrivProtocol.DES => new DESPrivacyProvider(pwd, auth),
                SnmpV3PrivProtocol.AES128 => new AESPrivacyProvider(pwd, auth),
                SnmpV3PrivProtocol.AES192 => new AES192PrivacyProvider(pwd, auth),
                SnmpV3PrivProtocol.AES256 => new AES256PrivacyProvider(pwd, auth),
                _ => throw new NotSupportedException($"Privacy protocol {priv} not supported.")
            };
        }

        // ── Replace BuildPrivacyProvider to use the two helpers ──────────────────────

        private GetRequestMessage BuildV3Request(IList<string> oids, IPrivacyProvider privacy, ISnmpMessage syncReport)
        {
            return new GetRequestMessage(
                VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(v3Credentials?.Username ?? string.Empty),
                OctetString.Empty,
                BuildVariables(oids),
                privacy,
                Messenger.MaxMessageSize,
                syncReport);
        }

        private IPrivacyProvider BuildPrivacyProvider()
        {
            IAuthenticationProvider auth = v3Credentials?.AuthProtocol switch
            {
                SnmpV3AuthProtocol.MD5 => new MD5AuthenticationProvider(
                                                new OctetString(v3Credentials.AuthPassword)),
                SnmpV3AuthProtocol.SHA1 => new SHA1AuthenticationProvider(
                                                new OctetString(v3Credentials.AuthPassword)),
                SnmpV3AuthProtocol.SHA256 => new SHA256AuthenticationProvider(
                                                new OctetString(v3Credentials.AuthPassword)),
                SnmpV3AuthProtocol.SHA384 => new SHA384AuthenticationProvider(
                                                new OctetString(v3Credentials.AuthPassword)),
                SnmpV3AuthProtocol.SHA512 => new SHA512AuthenticationProvider(
                                                new OctetString(v3Credentials.AuthPassword)),
                _ => throw new NotSupportedException($"Auth protocol {v3Credentials?.AuthProtocol} not supported.")
            };

            IPrivacyProvider privacy = v3Credentials.PrivProtocol switch
            {
                SnmpV3PrivProtocol.None => new DefaultPrivacyProvider(auth),
                SnmpV3PrivProtocol.DES => new DESPrivacyProvider(
                                                new OctetString(v3Credentials.PrivPassword), auth),
                SnmpV3PrivProtocol.AES128 => new AESPrivacyProvider(
                                                new OctetString(v3Credentials.PrivPassword), auth),
                SnmpV3PrivProtocol.AES192 => new AES192PrivacyProvider(
                                                new OctetString(v3Credentials.PrivPassword), auth),
                SnmpV3PrivProtocol.AES256 => new AES256PrivacyProvider(
                                                new OctetString(v3Credentials.PrivPassword), auth),
                _ => throw new NotSupportedException($"Privacy protocol {v3Credentials.PrivProtocol} not supported.")
            };

            return privacy;
        }

        private static IList<Variable> BuildVariables(IList<string> oids)
        {
            IList<Variable> variables = new List<Variable>();
            foreach (string oid in oids)
                variables.Add(new Variable(new ObjectIdentifier(oid)));
            return variables;
        }

        private static bool IsNoSuchInstance(ISnmpData data)
        {  
            return  data.TypeCode == SnmpType.NoSuchInstance || data.TypeCode == SnmpType.NoSuchObject || data.TypeCode == SnmpType.EndOfMibView;
        }

        private static string FormatMac(ISnmpData data)
        {
            if (data is OctetString octet)
            {
                byte[] bytes = octet.GetRaw();
                return bytes.Length == 6
                    ? BitConverter.ToString(bytes).Replace('-', ':')
                    : data.ToString() ?? string.Empty;
            }

            return data.ToString() ?? string.Empty;
        }
    }
}
