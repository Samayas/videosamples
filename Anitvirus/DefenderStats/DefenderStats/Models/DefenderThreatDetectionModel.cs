namespace DefenderStats.Models
{
    public class DefenderThreatDetectionModel
    {
        public string DetectionID { get; set; } = string.Empty;
        public long ThreatID { get; set; }
        public DateTime? InitialDetectionTime { get; set; }
        public DateTime? LastThreatStatusChangeTime { get; set; }
        public DateTime? RemediationTime { get; set; }
        public string DomainUser { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string[] Resources { get; set; } = new string[0];
        public byte DetectionSourceTypeID { get; set; }   // 0=Unknown 1=User 2=System 3=RealTime 4=IOAV
        public uint ThreatStatusID { get; set; }
        public uint ThreatStatusErrorCode { get; set; }
        public uint CleaningActionID { get; set; }
        public bool ActionSuccess { get; set; }
        public uint AdditionalActionsBitMask { get; set; }
        public byte CurrentThreatExecutionStatusID { get; set; }
        public string AMProductVersion { get; set; } = string.Empty;
    }
}
