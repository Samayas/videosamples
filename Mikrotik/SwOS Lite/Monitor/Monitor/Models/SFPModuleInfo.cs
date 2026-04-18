namespace Mikrotik.SwOSLite.Monitor.Models
{
    public class SFPModuleInfo
    {
        public int Index { get; set; }
        public string? Name { get; set; }
        public bool RxLoss { get; set; }
        public bool TxFault { get; set; }
        public int Wavelength { get; set; }
        public double Temperature { get; set; }
        public double SupplyVoltage { get; set; }
        public double TxCurrent { get; set; }
        public double TxPower { get; set; }
        public double RxPower { get; set; }
    }
}
