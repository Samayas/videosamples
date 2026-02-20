using System;
using System.Net;
using System.Text;

namespace STUNLibrary.Client
{
    public class STUNNetworkInfo
    {
        public IPAddress PublicIPAddress { get; set; }
        public int PublicPort { get; set; }
        public IPAddress LocalIPAddress { get; set; }
        public int LocalPort { get; set; }
        public TimeSpan Latency { get; set; }
        public string ServerSoftware { get; set; } = string.Empty;
        public string STUNServerUsed { get; set; } = string.Empty;

        public string NATType
        {
            get
            {
                if (PublicIPAddress == null || LocalIPAddress == null) return "Unknown";
                return PublicIPAddress.Equals(LocalIPAddress)
                    ? "No NAT (Direct Connection)"
                    : "Behind NAT";
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Public Endpoint: {PublicIPAddress}:{PublicPort}");
            sb.AppendLine($"Local Endpoint:  {LocalIPAddress}:{LocalPort}");
            sb.AppendLine($"NAT Type:        {NATType}");
            sb.AppendLine($"Latency:         {Latency.TotalMilliseconds:F2} ms");
            if (!string.IsNullOrEmpty(ServerSoftware))
                sb.AppendLine($"Server Software: {ServerSoftware}");
            return sb.ToString();
        }
    }
}
