namespace TR064.Model
{
    public sealed class FritzHostEntry
    {
        public FritzHostEntry(int index, string? hostName, string? ipAddress, string? macAddress, bool active, string interfaceType, int leaseTimeRemaining)
        {
            this.Index = index;
            this.HostName = hostName;
            this.IpAddress = ipAddress;
            this.MacAddress = macAddress;
            this.Active = active;
            this.InterfaceType = interfaceType;
            this.LeaseTimeRemaining = leaseTimeRemaining;
        }

        public int Index { get; }
        public string? HostName { get; } = string.Empty;
        public string? IpAddress { get; } = string.Empty;
        public string? MacAddress { get; } = string.Empty;
        public bool Active { get; }
        public string InterfaceType { get; }
        public int LeaseTimeRemaining { get; }

        public override string ToString()
        {
            return $"HostName : {HostName} - IpAddress : {IpAddress} - MacAddress : {MacAddress} - Active : {Active} - InterfaceType : {InterfaceType} - LeaseTimeRemaining : {LeaseTimeRemaining}";
        }
    }
}
