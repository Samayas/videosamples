namespace VigorSNMP.Model
{
    public static class Vigor167Oids
    {
        // ── MIB-II System ──────────────────────────────────────────────────────
        public const string SysDescr = "1.3.6.1.2.1.1.1.0";
        public const string SysUpTime = "1.3.6.1.2.1.1.3.0";
        public const string SysContact = "1.3.6.1.2.1.1.4.0";
        public const string SysName = "1.3.6.1.2.1.1.5.0";
        public const string SysLocation = "1.3.6.1.2.1.1.6.0";

        // ── DrayTek private MIB — device scalars ───────────────────────────────
        public const string RouterModel = "1.3.6.1.4.1.7367.3.1.0";
        public const string RouterRevision = "1.3.6.1.4.1.7367.3.2.0";
        public const string FwBuildDate = "1.3.6.1.4.1.7367.3.3.0";
        public const string DslChipVersion = "1.3.6.1.4.1.7367.3.4.0";
        public const string MemoryUsage = "1.3.6.1.4.1.7367.3.7.0";
        public const string LanMac = "1.3.6.1.4.1.7367.3.8.0";

        // ── RFC 5650 VDSL2-LINE-MIB — xdsl2LineStatusEntry COLUMN BASES ────────
        // Do NOT hardcode an instance suffix here — append .{ifIndex} at runtime.
        public const string Vdsl2LineActTransSys = "1.3.6.1.2.1.10.251.1.2.1.1.1";
        public const string Vdsl2LineActProfile = "1.3.6.1.2.1.10.251.1.2.1.1.2";
        public const string Vdsl2LineActMode = "1.3.6.1.2.1.10.251.1.2.1.1.3";
        public const string Vdsl2LinePwrMngState = "1.3.6.1.2.1.10.251.1.2.1.1.4";
        public const string Vdsl2LineLastStateDs = "1.3.6.1.2.1.10.251.1.2.1.1.5";
        public const string Vdsl2LineLastStateUs = "1.3.6.1.2.1.10.251.1.2.1.1.6";
        public const string Vdsl2LineCoDefects = "1.3.6.1.2.1.10.251.1.2.1.1.7";
        public const string Vdsl2LineCpeDefects = "1.3.6.1.2.1.10.251.1.2.1.1.8";
        public const string Vdsl2LineInitResult = "1.3.6.1.2.1.10.251.1.2.1.1.9";
        public const string Vdsl2LineSnrMgnDs = "1.3.6.1.2.1.10.251.1.2.1.1.10";
        public const string Vdsl2LineSnrMgnUs = "1.3.6.1.2.1.10.251.1.2.1.1.11";
        public const string Vdsl2LineAttenuationDs = "1.3.6.1.2.1.10.251.1.2.1.1.12";
        public const string Vdsl2LineAttenuationUs = "1.3.6.1.2.1.10.251.1.2.1.1.13";

        // ── RFC 5650 VDSL2-LINE-MIB — xdsl2ChStatusEntry COLUMN BASES ──────────
        // Append .{ifIndex}.1 (near-end/DS) or .{ifIndex}.2 (far-end/US) at runtime.
        public const string Vdsl2ChActDataRate = "1.3.6.1.2.1.10.251.1.2.2.1.2";
        public const string Vdsl2ChPrevDataRate = "1.3.6.1.2.1.10.251.1.2.2.1.3";
        public const string Vdsl2ChActDelay = "1.3.6.1.2.1.10.251.1.2.2.1.5";
        public const string Vdsl2ChActInp = "1.3.6.1.2.1.10.251.1.2.2.1.6";

        // ── RFC 5650 VDSL2-LINE-MIB — xdsl2PMLineCurrEntry COLUMN BASES ────────
        // Append .{ifIndex}.1 (near-end/CO) or .{ifIndex}.2 (far-end/CPE) at runtime.
        public const string Vdsl2Pm15MinEs = "1.3.6.1.2.1.10.251.1.4.1.1.5";
        public const string Vdsl2Pm15MinSes = "1.3.6.1.2.1.10.251.1.4.1.1.6";
        public const string Vdsl2Pm15MinLoss = "1.3.6.1.2.1.10.251.1.4.1.1.7";
        public const string Vdsl2Pm15MinUas = "1.3.6.1.2.1.10.251.1.4.1.1.8";

        // ── RFC 2662 ADSL-LINE-MIB (1.3.6.1.2.1.10.94.1.1) ───────────────────────
        // Confirmed present on Vigor167 fw 5.2.x — instance suffix is always .0
        // adslLineTable
        public const string AdslLineConfProfile = "1.3.6.1.2.1.10.94.1.1.1.1.4.0";

        // adslAtucPhysTable — CO / DSLAM side (downstream path)
        public const string AdslAtucSnrMgn = "1.3.6.1.2.1.10.94.1.1.2.1.4.0";  // dB, Integer32
        public const string AdslAtucAtn = "1.3.6.1.2.1.10.94.1.1.2.1.5.0";  // dB, Gauge32
        public const string AdslAtucStatus = "1.3.6.1.2.1.10.94.1.1.2.1.6.0";  // e.g. "SHOWTIME"
        public const string AdslAtucOutputPwr = "1.3.6.1.2.1.10.94.1.1.2.1.7.0";  // tenths dBm, Integer32
        public const string AdslAtucAttainRate = "1.3.6.1.2.1.10.94.1.1.2.1.8.0";  // bps, Gauge32

        // adslAturPhysTable — CPE / modem side (upstream path)
        public const string AdslAturChipVersion = "1.3.6.1.2.1.10.94.1.1.3.1.3.0";
        public const string AdslAturSnrMgn = "1.3.6.1.2.1.10.94.1.1.3.1.4.0";  // dB, Integer32
        public const string AdslAturAtn = "1.3.6.1.2.1.10.94.1.1.3.1.5.0";  // dB, Gauge32
        public const string AdslAturStatus = "1.3.6.1.2.1.10.94.1.1.3.1.6.0";  // e.g. "SHOWTIME"
        public const string AdslAturOutputPwr = "1.3.6.1.2.1.10.94.1.1.3.1.7.0";  // tenths dBm, Integer32
        public const string AdslAturAttainRate = "1.3.6.1.2.1.10.94.1.1.3.1.8.0";  // bps, Gauge32

        // adslAtucChanTable — downstream channel rates
        public const string AdslAtucChanDelay = "1.3.6.1.2.1.10.94.1.1.4.1.1.0";  // ms, Gauge32
        public const string AdslAtucChanTxRate = "1.3.6.1.2.1.10.94.1.1.4.1.2.0";  // bps, Gauge32  ← current DS rate
        public const string AdslAtucChanPrevRate = "1.3.6.1.2.1.10.94.1.1.4.1.3.0";  // bps, Gauge32

        // adslAturChanTable — upstream channel rates
        public const string AdslAturChanDelay = "1.3.6.1.2.1.10.94.1.1.5.1.1.0";  // ms, Gauge32
        public const string AdslAturChanTxRate = "1.3.6.1.2.1.10.94.1.1.5.1.2.0";  // bps, Gauge32  ← current US rate
        public const string AdslAturChanPrevRate = "1.3.6.1.2.1.10.94.1.1.5.1.3.0";  // bps, Gauge32

        // ── MIB-II ifTable — dynamic OID builder ───────────────────────────────
        public static string GetIfOid(string column, int ifIndex)
        {
            return $"1.3.6.1.2.1.2.2.1.{column}.{ifIndex}";
        }

        public static string GetIfXOid(string column, int ifIndex)
        {
            return $"1.3.6.1.2.1.31.1.1.1.{column}.{ifIndex}";
        }

        // Alias kept so LAN calls that use GetLanIfOid still compile
        public static string GetLanIfOid(string column, int ifIndex)
            => GetIfOid(column, ifIndex);

        public const string IfColDescr = "2";
        public const string IfColType = "3";
        public const string IfColSpeed = "5";
        public const string IfColOperStatus = "8";
        public const string IfColInOctets = "10";
        public const string IfColInDiscards = "13";
        public const string IfColInErrors = "14";
        public const string IfColOutOctets = "16";
        public const string IfColOutDiscards = "19";
        public const string IfColOutErrors = "20";
        public const string IfXColHCInOctets = "6";
        public const string IfXColHCOutOctets = "10";
    }
}
