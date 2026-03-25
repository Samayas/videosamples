namespace TR064.Model
{
    public sealed class FritzTimeInfo
    {
        public FritzTimeInfo(string? ntpServer1, string? ntpServer2, DateTime? currentLocalTime, string? localTimeZone, string? localTimeZoneName, bool? daylightSavingsUsed)
        {
            NtpServer1 = ntpServer1;
            NtpServer2 = ntpServer2;
            CurrentLocalTime = currentLocalTime;
            LocalTimeZone = localTimeZone;
            LocalTimeZoneName = localTimeZoneName;
            DaylightSavingsUsed = daylightSavingsUsed;
        }

        public string? NtpServer1 { get; } = string.Empty;
        public string? NtpServer2 { get; } = string.Empty;
        public DateTime? CurrentLocalTime { get; }
        public string? LocalTimeZone { get; }
        public string? LocalTimeZoneName { get; }
        public bool? DaylightSavingsUsed { get; }

        public override string ToString()
        {
            return $"NtpServer1 : {NtpServer1} - NtpServer2 : {NtpServer2} - CurrentLocalTime : {CurrentLocalTime} - LocalTimeZone : {LocalTimeZone} - LocalTimeZoneName : {LocalTimeZoneName} - DaylightSavingsUsed : {DaylightSavingsUsed}";
        }
    }
}
