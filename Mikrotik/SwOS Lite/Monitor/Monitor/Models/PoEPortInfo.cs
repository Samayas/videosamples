namespace Mikrotik.SwOSLite.Monitor.Models
{
    public class PoEPortInfo
    {
        public int PortIndex { get; set; }
        public string? PortName { get; set; }
        public string? AdminStatus { get; set; }
        public string? DetectionStatus { get; set; }
        public string? PowerPriority { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double Power { get; set; }
        public string? PowerPairs { get; set; }
        public string? PortType { get; set; }
    }
}
