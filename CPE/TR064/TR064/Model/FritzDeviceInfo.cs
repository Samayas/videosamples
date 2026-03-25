namespace TR064.Model
{
    public sealed class FritzDeviceInfo
    {
        public FritzDeviceInfo(string? manufacturer, string? modelName, string? serialNumber, string? softwareVersion, int uptimeSeconds)
        {
            this.Manufacturer = manufacturer;
            this.ModelName = modelName;
            this.SerialNumber = serialNumber;
            this.SoftwareVersion = softwareVersion;
            this.UptimeSeconds = uptimeSeconds;
        }

        public string? Manufacturer { get; } = string.Empty;
        public string? ModelName { get; } = string.Empty;
        public string? SerialNumber { get; } = string.Empty;
        public string? SoftwareVersion { get; } = string.Empty;
        public int UptimeSeconds { get; }

        public override string ToString()
        {
            return $"Manufacturer : {Manufacturer} - ModelName : {ModelName} - SerialNumber : {SerialNumber} - SoftwareVersion : {SoftwareVersion} - UptimeSeconds : {UptimeSeconds}";
        }
    }
}
