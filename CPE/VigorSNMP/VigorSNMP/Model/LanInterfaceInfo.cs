namespace VigorSNMP.Model
{
    public sealed class LanInterfaceInfo
    {
        public int IfIndex { get; set; }
        public string Description { get; set; } = string.Empty;
        public long SpeedBps { get; set; }
        public int OperStatus { get; set; }
        public long InOctets { get; set; }
        public long OutOctets { get; set; }
        public long InErrors { get; set; }
        public long OutErrors { get; set; }
        public long InDiscards { get; set; }
        public long OutDiscards { get; set; }

        public double SpeedMbps => SpeedBps / 1_000_000.0;
        public double TotalInGb => InOctets / 1_073_741_824.0;
        public double TotalOutGb => OutOctets / 1_073_741_824.0;

        public string OperStatusText => OperStatus switch
        {
            1 => "Up",
            2 => "Down",
            3 => "Testing",
            5 => "Dormant",
            _ => $"Unknown ({OperStatus})"
        };
    }
}
