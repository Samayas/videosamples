using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;

namespace Mikrotik.SwOSLite.Monitor.Services
{
    public class SNMPManager
    {
        private readonly string ipAddress;
        private readonly string community;
        private readonly int port;
        private readonly int timeout;

        public SNMPManager(string ipAddress, string community = "public", int port = 161, int timeout = 5000)
        {
            this.ipAddress = ipAddress;
            this.community = community;
            this.port = port;
            this.timeout = timeout;
        }

        public string GetValue(string oid)
        {
            try
            {
                IList<Variable> result = Messenger.Get(VersionCode.V2, GetEndpoint(), GetCommunity(), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, timeout);

                if (result != null && result.Count > 0)
                {
                    return result[0].Data.ToString();
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        public Variable? GetVariable(string oid)
        {
            try
            {
                IList<Variable> result = Messenger.Get(VersionCode.V2, GetEndpoint(), GetCommunity(), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, timeout);

                return result?.FirstOrDefault() ?? null;
            }
            catch
            {
            }

            return null;
        }

        public Dictionary<string, string> GetMultiple(List<string> oids)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            try
            {
                List<Variable> variables = oids.Select(oid => new Variable(new ObjectIdentifier(oid))).ToList();

                IList<Variable> result = Messenger.Get(VersionCode.V2, GetEndpoint(), GetCommunity(), variables, timeout);

                for (int count = 0; count < oids.Count && count < result.Count; count++)
                {
                    results[oids[count]] = result[count].Data.ToString();
                }
            }
            catch
            {
            }

            return results;
        }

        public List<Variable> Walk(string oid)
        {
            List<Variable> results = new List<Variable>();
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                Messenger.Walk(VersionCode.V2, endpoint, GetCommunity(), new ObjectIdentifier(oid), results, timeout, WalkMode.WithinSubtree);
            }
            catch
            {
            }

            return results;
        }

        public List<Variable> BulkWalk(string oid, int maxRepetitions = 10)
        {
            List<Variable> results = new List<Variable>();
            try
            {
                Messenger.BulkWalk(VersionCode.V2,
                    GetEndpoint(),
                    GetCommunity(),
                    OctetString.Empty,
                    new ObjectIdentifier(oid),
                    results,
                    timeout,
                    maxRepetitions,
                    WalkMode.WithinSubtree,
                    null,
                    null);
            }
            catch
            {
            }

            return results;
        }

        public string ConvertBytesToMac(byte[] macBytes)
        {
            if (macBytes == null || macBytes.Length == 0)
            {
                return string.Empty;
            }

            return BitConverter.ToString(macBytes).Replace("-", ":");
        }

        public string ConvertBytesToMac(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return string.Empty;
            }

            try
            {
                hexString = hexString.Replace(" ", "").Replace(":", "").Replace("-", "");
                if (hexString.Length < 12)
                {
                    return hexString;
                }

                return string.Join(":", Enumerable.Range(0, hexString.Length / 2).Select(i => hexString.Substring(i * 2, 2)));
            }
            catch
            {
                return hexString;
            }
        }

        public string GetMacAddress(string oid)
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(this.ipAddress), port);
                IList<Variable> result = Messenger.Get(VersionCode.V2, endpoint, GetCommunity(),
                    new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, timeout);

                if (result != null && result.Count > 0)
                {
                    // FIX: Cast the data to OctetString and grab the .Value property (which is a byte[])
                    // Do NOT use .ToString() on the Data object itself.
                    if (result[0].Data is OctetString octet)
                    {
                        return ConvertBytesToMac(octet.GetRaw());
                    }
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        public string ConvertUpTime(string timeticks)
        {
            try
            {
                if (long.TryParse(timeticks, out long ticks))
                {
                    var timeSpan = TimeSpan.FromMilliseconds(ticks * 10);
                    return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
                }
            }
            catch
            {
            }

            return timeticks;
        }

        public string GetInterfaceStatus(string statusCode)
        {
            return statusCode switch
            {
                "1" => "up",
                "2" => "down",
                "3" => "testing",
                "4" => "unknown",
                "5" => "dormant",
                "6" => "notPresent",
                "7" => "lowerLayerDown",
                _ => statusCode
            };
        }

        public string GetInterfaceType(string typeCode)
        {
            return typeCode switch
            {
                "6" => "ethernet",
                "24" => "softwareLoopback",
                "131" => "tunnel",
                "135" => "vlan",
                "136" => "l3ipvlan",
                "161" => "ieee8023adLag",
                "244" => "wwanPP",
                _ => $"type-{typeCode}"
            };
        }

        private IPEndPoint GetEndpoint()
        {
            return new IPEndPoint(IPAddress.Parse(this.ipAddress), this.port);
        }

        private OctetString GetCommunity()
        {
            return new OctetString(community);
        }
    }
}
