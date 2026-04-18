namespace Mikrotik.SwOSLite.Monitor.Models
{
    public class InterfaceInfo
    {
        public int Index { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public long Speed { get; set; }
        public string? PhysAddress { get; set; }
        public string? AdminStatus { get; set; }
        public string? OperStatus { get; set; }
        public long InOctets { get; set; }
        public long OutOctets { get; set; }
        public long InErrors { get; set; }
        public long OutErrors { get; set; }
        public long InDiscards { get; set; }
        public long OutDiscards { get; set; }
        public string? Description { get; set; }
    }
}
