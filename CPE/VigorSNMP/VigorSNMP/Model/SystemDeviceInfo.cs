namespace VigorSNMP.Model
{
    public sealed class SystemDeviceInfo
    {
        public string SystemDescription { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string SystemContact { get; set; } = string.Empty;
        public string SystemLocation { get; set; } = string.Empty;
        public TimeSpan SystemUpTime { get; set; }
        public string RouterModel { get; set; } = string.Empty;
        public string FirmwareRevision { get; set; } = string.Empty;
        public string FirmwareBuildDate { get; set; } = string.Empty;
        public string DslChipsetVersion { get; set; } = string.Empty;
        public int MemoryUsagePercent { get; set; }
        public string LanMacAddress { get; set; } = string.Empty;

        public string UptimeFormatted => $"{(int)SystemUpTime.TotalDays}d {SystemUpTime.Hours}h {SystemUpTime.Minutes}m {SystemUpTime.Seconds}s";
    }
}
