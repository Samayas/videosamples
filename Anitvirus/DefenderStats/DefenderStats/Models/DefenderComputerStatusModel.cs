namespace DefenderStats.Models
{
    public class DefenderComputerStatusModel
    {
        // Versions
        public string AMEngineVersion { get; set; } = string.Empty;
        public string AMProductVersion { get; set; } = string.Empty;
        public string AMServiceVersion { get; set; } = string.Empty;
        public string NISEngineVersion { get; set; } = string.Empty;

        // Service flags
        public bool AMServiceEnabled { get; set; }
        public bool AntivirusEnabled { get; set; }
        public bool AntispywareEnabled { get; set; }
        public bool RealTimeProtectionEnabled { get; set; }
        public bool BehaviorMonitorEnabled { get; set; }
        public bool IoavProtectionEnabled { get; set; }        // Downloads/attachments scan
        public bool OnAccessProtectionEnabled { get; set; }   // File/program activity monitoring
        public bool NISEnabled { get; set; }                  // Network Inspection System

        // Computer state
        public string ComputerID { get; set; } = string.Empty;
        public uint ComputerState { get; set; }

        // Antivirus signatures
        public string AntivirusSignatureVersion { get; set; } = string.Empty;
        public uint AntivirusSignatureAge { get; set; }
        public DateTime? AntivirusSignatureLastUpdated { get; set; }

        // Antispyware signatures
        public string AntispywareSignatureVersion { get; set; } = string.Empty;
        public uint AntispywareSignatureAge { get; set; }
        public DateTime? AntispywareSignatureLastUpdated { get; set; }

        // NIS signatures
        public string NISSignatureVersion { get; set; } = string.Empty;
        public uint NISSignatureAge { get; set; }
        public DateTime? NISSignatureLastUpdated { get; set; }

        // Full scan
        public DateTime? FullScanStartTime { get; set; }
        public DateTime? FullScanEndTime { get; set; }
        public uint FullScanAge { get; set; }
        public byte LastFullScanSource { get; set; }   // 0=Unknown 1=User 2=System 3=RealTime 4=IOAV

        // Quick scan
        public DateTime? QuickScanStartTime { get; set; }
        public DateTime? QuickScanEndTime { get; set; }
        public uint QuickScanAge { get; set; }
        public byte LastQuickScanSource { get; set; }
        public byte RealTimeScanDirection { get; set; }
    }
}
