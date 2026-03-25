namespace VigorSNMP.Model
{
    public sealed class Vdsl2LineStatusInfo
    {
        // RFC 5650 fields
        public int ActTransmissionSystem { get; set; }
        public int ActProfile { get; set; }
        public int ActMode { get; set; }
        public int PowerManagementState { get; set; }
        public int LastStateDs { get; set; }
        public int LastStateUs { get; set; }
        public int CoDefects { get; set; }  // bitmask: bit0=LOS, bit1=LOF, bit2=LPR
        public int CpeDefects { get; set; }  // same bitmask
        public int InitResult { get; set; }
        // RFC 5650 store
        public double SnrMarginDsDb { get; set; }  // 0.1 dB units converted to dB
        public double SnrMarginUsDb { get; set; }
        public double AttenuationDsDb { get; set; }
        public double AttenuationUsDb { get; set; }
        // RFC 2662 fields
        public string LineStatusCo { get; set; } = string.Empty;  // e.g. "SHOWTIME"
        public string LineStatusCpe { get; set; } = string.Empty;
        public double CoDsOutputPowerDbm { get; set; }  // adslAtucOutputPwr / 10
        public double CpeUsOutputPowerDbm { get; set; }  // adslAturOutputPwr / 10
        public long AttainableDsRateBps { get; set; }  // adslAtucAttainRate
        public long AttainableUsRateBps { get; set; }  // adslAturAttainRate
        // Populated from RFC 2662 adslLineConfProfile when RFC 5650 is absent
        public string AdslRawProfileName { get; set; } = string.Empty;
        public string AdslAnnex { get; set; } = string.Empty;

        public string DisplayProfile => !string.IsNullOrEmpty(AdslRawProfileName) ? AdslRawProfileName : ActProfileText;
        public double AttainableDsRateMbps => AttainableDsRateBps / 1_000_000.0;
        public double AttainableUsRateMbps => AttainableUsRateBps / 1_000_000.0;

        public bool CoHasLos => (CoDefects & 0x01) != 0;
        public bool CoHasLof => (CoDefects & 0x02) != 0;
        public bool CoHasLpr => (CoDefects & 0x04) != 0;
        public bool CpeHasLos => (CpeDefects & 0x01) != 0;
        public bool CpeHasLof => (CpeDefects & 0x02) != 0;
        public bool CpeHasLpr => (CpeDefects & 0x04) != 0;

        public string ActProfileText => ActProfile switch
        {
            1 => "8a",
            2 => "8b",
            3 => "8c",
            4 => "8d",
            5 => "12a",
            6 => "12b",
            7 => "17a",
            8 => "30a",
            9 => "35b",
            _ => $"Unknown ({ActProfile})"
        };

        public string PowerManagementStateText => PowerManagementState switch
        {
            1 => "L0 – Full power on",
            2 => "L2 – Low power",
            3 => "L3 – Sleep / powered down",
            _ => $"Unknown ({PowerManagementState})"
        };

        public string InitResultText => InitResult switch
        {
            0 => "No failure",
            1 => "Configuration error",
            2 => "Configuration not feasible",
            3 => "Communication problem",
            4 => "No far-end response",
            5 => "No lock",
            6 => "No training",
            7 => "Failed show time",
            _ => $"Code {InitResult}"
        };
    }
}
