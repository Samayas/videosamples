namespace Mikrotik.SwOSLite.Monitor.Models
{
    public class STPInfo
    {
        public string? ProtocolSpecification { get; set; }
        public int Priority { get; set; }
        public string? DesignatedRoot { get; set; }
        public int RootCost { get; set; }
        public int RootPort { get; set; }
        public int MaxAge { get; set; }
        public int HelloTime { get; set; }
        public int ForwardDelay { get; set; }
        public long TopologyChanges { get; set; }
        public long TimeSinceTopologyChange { get; set; }
    }
}
