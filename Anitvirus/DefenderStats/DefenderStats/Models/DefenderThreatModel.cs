namespace DefenderStats.Models
{
    public class DefenderThreatModel
    {
        public long ThreatID { get; set; }
        public string ThreatName { get; set; } = string.Empty;
        public byte SeverityID { get; set; }       // 0=Unknown 1=Low 2=Moderate 4=High 5=Severe
        public byte CategoryID { get; set; }
        public byte TypeID { get; set; }
        public bool IsActive { get; set; }
        public bool RollupStatus { get; set; }
        public bool IsServiceStopping { get; set; }
        public long DidThreatExecute { get; set; }
        public uint DefaultActionID { get; set; }
    }
}
