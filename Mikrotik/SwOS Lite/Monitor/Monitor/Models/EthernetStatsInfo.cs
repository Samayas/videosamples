namespace Mikrotik.SwOSLite.Monitor.Models
{
    public class EthernetStatsInfo
    {
        public int InterfaceIndex { get; set; }
        public string? InterfaceName { get; set; }
        public string? DuplexStatus { get; set; }
        public long AlignmentErrors { get; set; }
        public long FCSErrors { get; set; }
        public long SingleCollisions { get; set; }
        public long MultipleCollisions { get; set; }
        public long LateCollisions { get; set; }
        public long ExcessiveCollisions { get; set; }
        public long DeferredTransmissions { get; set; }
        public long FrameTooLongs { get; set; }
        public long InMulticastPkts { get; set; }
        public long InBroadcastPkts { get; set; }
        public long OutMulticastPkts { get; set; }
        public long OutBroadcastPkts { get; set; }
    }
}
