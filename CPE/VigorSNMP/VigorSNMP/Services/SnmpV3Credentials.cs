namespace VigorSNMP.Services
{
    public sealed class SnmpV3Credentials
    {
        public string Username { get; }
        public string AuthPassword { get; }
        public SnmpV3AuthProtocol AuthProtocol { get; }
        public string PrivPassword { get; }
        public SnmpV3PrivProtocol PrivProtocol { get; }

        // authNoPriv — authenticated, not encrypted
        public SnmpV3Credentials(string username, string authPassword, SnmpV3AuthProtocol authProtocol = SnmpV3AuthProtocol.SHA256)
        {
            Username = username;
            AuthPassword = authPassword;
            AuthProtocol = authProtocol;
            PrivPassword = string.Empty;
            PrivProtocol = SnmpV3PrivProtocol.None;
        }

        // authPriv — authenticated and encrypted
        public SnmpV3Credentials(string username, string authPassword, SnmpV3AuthProtocol authProtocol, string privPassword, SnmpV3PrivProtocol privProtocol)
        {
            Username = username;
            AuthPassword = authPassword;
            AuthProtocol = authProtocol;
            PrivPassword = privPassword;
            PrivProtocol = privProtocol;
        }
    }
}
