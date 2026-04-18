using Mikrotik.SwOSLite.Monitor.Models;
using Lextm.SharpSnmpLib;

namespace Mikrotik.SwOSLite.Monitor.Services
{
    public class MikroTikSNMPService
    {
        private readonly SNMPManager snmpManager;

        public MikroTikSNMPService(string ipAddress, string community = "public", int port = 161)
        {
            this.snmpManager = new SNMPManager(ipAddress, community, port);
        }

        public DeviceInfo GetDeviceInfo()
        {
            DeviceInfo deviceInfo = new DeviceInfo
            {
                SystemDescription = this.snmpManager.GetValue(MikroTikOIDs.SysDescr),
                SystemObjectID = this.snmpManager.GetValue(MikroTikOIDs.SysObjectID),
                SystemUpTime = this.snmpManager.ConvertUpTime(this.snmpManager.GetValue(MikroTikOIDs.SysUpTime)),
                SystemContact = this.snmpManager.GetValue(MikroTikOIDs.SysContact),
                SystemName = this.snmpManager.GetValue(MikroTikOIDs.SysName),
                SystemLocation = this.snmpManager.GetValue(MikroTikOIDs.SysLocation),
                SerialNumber = this.snmpManager.GetValue(MikroTikOIDs.MtxrSerialNumber),
                FirmwareVersion = this.snmpManager.GetValue(MikroTikOIDs.MtxrFirmwareVersion)
            };

            return deviceInfo;
        }

        public IList<InterfaceInfo> GetInterfaces()
        {
            IList<InterfaceInfo> interfaces = new List<InterfaceInfo>();

            List<Variable> ifIndexes = this.snmpManager.Walk(MikroTikOIDs.IfIndex);

            foreach (Variable ifIndex in ifIndexes)
            {
                int index = Convert.ToInt32(ifIndex.Data.ToString());
                InterfaceInfo interfaceInfo = new InterfaceInfo { Index = index };

                try
                {
                    interfaceInfo.Name = this.snmpManager.GetValue($"{MikroTikOIDs.IfName}.{index}");
                    if (string.IsNullOrEmpty(interfaceInfo.Name))
                    {
                        interfaceInfo.Name = this.snmpManager.GetValue($"{MikroTikOIDs.IfDescr}.{index}");
                    }

                    string typeCode = this.snmpManager.GetValue($"{MikroTikOIDs.IfType}.{index}");
                    interfaceInfo.Type = this.snmpManager.GetInterfaceType(typeCode);

                    string operStatus = this.snmpManager.GetValue($"{MikroTikOIDs.IfOperStatus}.{index}");
                    interfaceInfo.OperStatus = this.snmpManager.GetInterfaceStatus(operStatus);

                    if (interfaceInfo.OperStatus == "up")
                    {
                        long detectedSpeed = 0;

                        // Attempt A: Try ifHighSpeed (The standard for >1Gbps, returns Mbps)
                        var highSpeedVar = this.snmpManager.GetVariable($"{MikroTikOIDs.IfHighSpeed}.{index}");
                        if (highSpeedVar != null && highSpeedVar.Data != null)
                        {
                            string hValStr = highSpeedVar.Data.ToString();
                            if (long.TryParse(hValStr, out long val))
                            {
                                // ifHighSpeed is in Mbps. 
                                // We convert to bps for consistency.
                                detectedSpeed = val * 1_000_000;
                            }
                        }

                        interfaceInfo.Speed = detectedSpeed;
                    }

                    string mac = this.snmpManager.GetMacAddress($"{MikroTikOIDs.IfPhysAddress}.{index}");
                    interfaceInfo.PhysAddress = mac;

                    string adminStatus = this.snmpManager.GetValue($"{MikroTikOIDs.IfAdminStatus}.{index}");
                    interfaceInfo.AdminStatus = this.snmpManager.GetInterfaceStatus(adminStatus);

                    interfaceInfo.Description = this.snmpManager.GetValue($"{MikroTikOIDs.IfAlias}.{index}");

                    string inOctets = this.snmpManager.GetValue($"{MikroTikOIDs.IfHCInOctets}.{index}");
                    if (string.IsNullOrEmpty(inOctets))
                    {
                        inOctets = this.snmpManager.GetValue($"{MikroTikOIDs.IfInOctets}.{index}");
                    }
                    if (long.TryParse(inOctets, out long inOctetsValue))
                    {
                        interfaceInfo.InOctets = inOctetsValue;
                    }

                    string outOctets = this.snmpManager.GetValue($"{MikroTikOIDs.IfHCOutOctets}.{index}");
                    if (string.IsNullOrEmpty(outOctets))
                    {
                        outOctets = this.snmpManager.GetValue($"{MikroTikOIDs.IfOutOctets}.{index}");
                    }
                    if (long.TryParse(outOctets, out long outOctetsValue))
                    {
                        interfaceInfo.OutOctets = outOctetsValue;
                    }
                    
                    string inErrors = this.snmpManager.GetValue($"{MikroTikOIDs.IfInErrors}.{index}");
                    if (long.TryParse(inErrors, out long inErrorsValue))
                    {
                        interfaceInfo.InErrors = inErrorsValue;
                    }

                    string outErrors = this.snmpManager.GetValue($"{MikroTikOIDs.IfOutErrors}.{index}");
                    if (long.TryParse(outErrors, out long outErrorsValue))
                    {
                        interfaceInfo.OutErrors = outErrorsValue;
                    }

                    string inDiscards = this.snmpManager.GetValue($"{MikroTikOIDs.IfInDiscards}.{index}");
                    if (long.TryParse(inDiscards, out long inDiscardsValue))
                    {
                        interfaceInfo.InDiscards = inDiscardsValue;
                    }

                    string outDiscards = this.snmpManager.GetValue($"{MikroTikOIDs.IfOutDiscards}.{index}");
                    if (long.TryParse(outDiscards, out long outDiscardsValue))
                    {
                        interfaceInfo.OutDiscards = outDiscardsValue;
                    }

                    interfaces.Add(interfaceInfo);
                }
                catch
                {
                }
            }

            return interfaces;
        }

        public IList<SwitchHost> GetSwitchHosts()
        {
            IList<SwitchHost> hosts = new List<SwitchHost>();

            try
            {
                List<Variable> macEntries = this.snmpManager.Walk(MikroTikOIDs.HostsMacTableOid);

                if (macEntries == null || macEntries.Count == 0)
                {
                    return hosts;
                }

                foreach (Variable entry in macEntries)
                {
                    try
                    {
                        // 1. Get the full OID and extract the index part
                        string currentFullOid = entry.Id.ToString();

                        string mac = this.snmpManager.GetMacAddress(currentFullOid);
                        if (string.IsNullOrEmpty(mac))
                        {
                            continue;
                        }

                        int baseOidLength = MikroTikOIDs.HostsMacTableOid.Length;
                        string indexPart = currentFullOid.Substring(baseOidLength);

                        if (!indexPart.StartsWith("."))
                        {
                            indexPart = "." + indexPart;
                        }

                        string combinedPortOid = MikroTikOIDs.HostsPortTableOid + indexPart;

                        combinedPortOid = combinedPortOid.Replace("..", ".");

                        // 3. Get the Port using the index part
                        string portStr = this.snmpManager.GetValue(combinedPortOid);

                        hosts.Add(new SwitchHost
                        {
                            MACAddress = mac,
                            Port = int.TryParse(portStr, out int p) ? p : 0
                        });
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return hosts;
        }

        public IDictionary<string, string> GetHealthStatus()
        {
            IDictionary<string, string> health = new Dictionary<string, string>();

            try
            {
                health["Temperature"] = this.snmpManager.GetValue(MikroTikOIDs.MtxrHlTemperature);
            }
            catch
            {
            }

            return health;
        }     

        public List<SFPModuleInfo> GetSFPModules()
        {
            List<SFPModuleInfo> sfpModules = new List<SFPModuleInfo>();

            try
            {
                List<Variable> sfpData = this.snmpManager.Walk(MikroTikOIDs.MtxrOpticalTable);

                IEnumerable<IGrouping<string, Variable>> sfpGroups = sfpData
                    .GroupBy(v =>
                    {
                        string[] parts = v.Id.ToString().Split('.');
                        return parts.Length >= 2 ? parts[parts.Length - 1] : "0";
                    });

                foreach (IGrouping<string, Variable> group in sfpGroups)
                {
                    try
                    {
                        SFPModuleInfo sfpModuleInfo = new SFPModuleInfo
                        {
                            Index = int.TryParse(group.Key, out int idx) ? idx : 0
                        };

                        foreach (Variable variable in group)
                        {
                            string oid = variable.Id.ToString();
                            if (oid.StartsWith(MikroTikOIDs.MtxrOpticalName))
                            {
                                sfpModuleInfo.Name = variable.Data.ToString();
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalRxLoss))
                            {
                                sfpModuleInfo.RxLoss = variable.Data.ToString() == "1";
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalTxFault))
                            {
                                sfpModuleInfo.TxFault = variable.Data.ToString() == "1";
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalWavelength))
                            {
                                sfpModuleInfo.Wavelength = int.TryParse(variable.Data.ToString(), out int w) ? w : 0;
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalTemperature))
                            {
                                sfpModuleInfo.Temperature = double.TryParse(variable.Data.ToString(), out double t) ? t / 10.0 : 0;
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalSupplyVoltage))
                            {
                                sfpModuleInfo.SupplyVoltage = double.TryParse(variable.Data.ToString(), out double v) ? v / 1000.0 : 0;
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalTxCurrent))
                            {
                                sfpModuleInfo.TxCurrent = double.TryParse(variable.Data.ToString(), out double tc) ? tc : 0;
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalTxPower))
                            {
                                sfpModuleInfo.TxPower = double.TryParse(variable.Data.ToString(), out double tp) ? tp / 1000.0 : 0;
                            }
                            else if (oid.StartsWith(MikroTikOIDs.MtxrOpticalRxPower))
                            {
                                sfpModuleInfo.RxPower = double.TryParse(variable.Data.ToString(), out double rp) ? rp / 1000.0 : 0;
                            }
                        }

                        sfpModules.Add(sfpModuleInfo);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return sfpModules;
        }

     
        public STPInfo GetSTPInfo()
        {
            STPInfo stpInfo = new STPInfo();

            try
            {
                stpInfo.ProtocolSpecification = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpProtocolSpecification);

                string priority = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpPriority);
                stpInfo.Priority = int.TryParse(priority, out int p) ? p : 0;

                var rootVar = this.snmpManager.GetVariable(MikroTikOIDs.Dot1dStpDesignatedRoot);
                if (rootVar != null && rootVar.Data is OctetString octet)
                {
                    byte[] bytes = octet.GetRaw();

                    if (bytes.Length >= 8)
                    {
                        // Extract Priority (Bytes 0 and 1) and convert to Hex string like "0x0100"
                        string hexPriority = $"0x{bytes[0]:X2}{bytes[1]:X2}";

                        // Extract MAC Address (Bytes 2 through 7)
                        string macAddress = BitConverter.ToString(bytes, 2, 6).Replace("-", ":");

                        // Combine them for a human-readable string: "0x0100:14:00:02:00:0F:00"
                        stpInfo.DesignatedRoot = $"{hexPriority}:{macAddress}";
                    }
                    else
                    {
                        stpInfo.DesignatedRoot = "Invalid Bridge ID";
                    }
                }
                else
                {
                    stpInfo.DesignatedRoot = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpDesignatedRoot);
                }

                string rootCost = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpRootCost);
                stpInfo.RootCost = int.TryParse(rootCost, out int rc) ? rc : 0;

                string rootPort = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpRootPort);
                stpInfo.RootPort = int.TryParse(rootPort, out int rp) ? rp : 0;

                string maxAge = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpMaxAge);
                stpInfo.MaxAge = int.TryParse(maxAge, out int ma) ? ma : 0;

                string helloTime = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpHelloTime);
                stpInfo.HelloTime = int.TryParse(helloTime, out int ht) ? ht : 0;

                string forwardDelay = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpForwardDelay);
                stpInfo.ForwardDelay = int.TryParse(forwardDelay, out int fd) ? fd : 0;

                string topChanges = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpTopChanges);
                stpInfo.TopologyChanges = long.TryParse(topChanges, out long tc) ? tc : 0;

                string timeSince = this.snmpManager.GetValue(MikroTikOIDs.Dot1dStpTimeSinceTopologyChange);
                stpInfo.TimeSinceTopologyChange = long.TryParse(timeSince, out long ts) ? ts : 0;
            }
            catch
            {
            }

            return stpInfo;
        }

        public List<EthernetStatsInfo> GetEthernetStats()
        {
            List<EthernetStatsInfo> ethernetStats = new List<EthernetStatsInfo>();

            try
            {
                List<Variable> interfaces = this.snmpManager.Walk(MikroTikOIDs.IfIndex);

                foreach (Variable interfaceVariable in interfaces)
                {
                    try
                    {
                        int ifIndex = int.TryParse(interfaceVariable.Data.ToString(), out int idx) ? idx : 0;

                        EthernetStatsInfo ethernetStatsInfo = new EthernetStatsInfo
                        {
                            InterfaceIndex = ifIndex,
                            InterfaceName = this.snmpManager.GetValue($"{MikroTikOIDs.IfName}.{ifIndex}")
                        };

                        string duplexStatus = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsDuplexStatus}.{ifIndex}");
                        ethernetStatsInfo.DuplexStatus = duplexStatus switch
                        {
                            "1" => "unknown",
                            "2" => "half-duplex",
                            "3" => "full-duplex",
                            _ => duplexStatus
                        };

                        string alignErr = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsAlignmentErrors}.{ifIndex}");
                        ethernetStatsInfo.AlignmentErrors = long.TryParse(alignErr, out long ae) ? ae : 0;

                        string fcsErr = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsFCSErrors}.{ifIndex}");
                        ethernetStatsInfo.FCSErrors = long.TryParse(fcsErr, out long fe) ? fe : 0;

                        string singleColl = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsSingleCollisionFrames}.{ifIndex}");
                        ethernetStatsInfo.SingleCollisions = long.TryParse(singleColl, out long sc) ? sc : 0;

                        string multiColl = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsMultipleCollisionFrames}.{ifIndex}");
                        ethernetStatsInfo.MultipleCollisions = long.TryParse(multiColl, out long mc) ? mc : 0;

                        string lateColl = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsLateCollisions}.{ifIndex}");
                        ethernetStatsInfo.LateCollisions = long.TryParse(lateColl, out long lc) ? lc : 0;

                        string excessColl = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsExcessiveCollisions}.{ifIndex}");
                        ethernetStatsInfo.ExcessiveCollisions = long.TryParse(excessColl, out long ec) ? ec : 0;

                        string deferred = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsDeferredTransmissions}.{ifIndex}");
                        ethernetStatsInfo.DeferredTransmissions = long.TryParse(deferred, out long dt) ? dt : 0;

                        string frameTooLong = this.snmpManager.GetValue($"{MikroTikOIDs.Dot3StatsFrameTooLongs}.{ifIndex}");
                        ethernetStatsInfo.FrameTooLongs = long.TryParse(frameTooLong, out long ftl) ? ftl : 0;

                        string inMcast = this.snmpManager.GetValue($"{MikroTikOIDs.IfInMulticastPkts}.{ifIndex}");
                        ethernetStatsInfo.InMulticastPkts = long.TryParse(inMcast, out long im) ? im : 0;

                        string inBcast = this.snmpManager.GetValue($"{MikroTikOIDs.IfInBroadcastPkts}.{ifIndex}");
                        ethernetStatsInfo.InBroadcastPkts = long.TryParse(inBcast, out long ib) ? ib : 0;

                        string outMcast = this.snmpManager.GetValue($"{MikroTikOIDs.IfOutMulticastPkts}.{ifIndex}");
                        ethernetStatsInfo.OutMulticastPkts = long.TryParse(outMcast, out long om) ? om : 0;

                        string outBcast = this.snmpManager.GetValue($"{MikroTikOIDs.IfOutBroadcastPkts}.{ifIndex}");
                        ethernetStatsInfo.OutBroadcastPkts = long.TryParse(outBcast, out long ob) ? ob : 0;

                        ethernetStats.Add(ethernetStatsInfo);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return ethernetStats;
        }
    }
}
