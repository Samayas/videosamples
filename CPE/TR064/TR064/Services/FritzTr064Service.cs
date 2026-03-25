using System;
using System.Net;
using System.Text;
using System.Xml.Linq;
using TR064.Model;

namespace TR064.Services
{
    public sealed class FritzTr064Service
    {
        private readonly Uri baseUri;
        private readonly HttpClient httpClient;

        public FritzTr064Service(Uri baseUri, string userName, string password, bool ignoreTlsCertificateErrors = false)
        {
            this.baseUri = baseUri;

            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(userName, password);
            handler.PreAuthenticate = false;

            if (ignoreTlsCertificateErrors)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            this.httpClient = new HttpClient(handler);
            this.httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<string?> GetExternalIpAddressAsync(CancellationToken cancellationToken = default)
        {
            string serviceType = "urn:dslforum-org:service:WANIPConnection:1";
            string action = "GetExternalIPAddress";
            string controlUrl = "/upnp/control/wanipconnection";

            try
            {
                XDocument response = await this.CallSoapAsync(serviceType, action, controlUrl, null, cancellationToken);
                XNamespace ns = serviceType;

                IEnumerable<XElement> ipElements = response.Descendants(ns + "NewExternalIPAddress");
                XElement? ipElement = ipElements.FirstOrDefault();
                return ipElement?.Value;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception)
            {
                // In bridge mode this will often be a SOAP fault with HTTP 500.
                return null;
            }
        }

        public async Task<FritzDeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default)
        {
            string serviceType = "urn:dslforum-org:service:DeviceInfo:1";
            string action = "GetInfo";
            string controlUrl = "/upnp/control/deviceinfo";

            XDocument doc = await this.CallSoapAsync(
                serviceType,
                action,
                controlUrl,
                null,
                cancellationToken);

            string? manufacturer = FindFirstValue(doc, "NewManufacturerName");
            string? modelName = FindFirstValue(doc, "NewModelName");
            string? serialNumber = FindFirstValue(doc, "NewSerialNumber") ;
            string? softwareVersion = FindFirstValue(doc, "NewSoftwareVersion");
            string uptimeText = FindFirstValue(doc, "NewUpTime") ?? "0";

            int uptimeSeconds = ParseInt32Safe(uptimeText);

            FritzDeviceInfo info = new FritzDeviceInfo(
                manufacturer,
                modelName,
                serialNumber,
                softwareVersion,
                uptimeSeconds);

            return info;
        }

        public async Task<IList<FritzHostEntry>> GetHostsAsync(int maxHosts, CancellationToken cancellationToken = default)
        {
            string serviceType = "urn:dslforum-org:service:Hosts:1";
            string controlUrl = "/upnp/control/hosts";

            XDocument countDoc = await this.CallSoapAsync(
                serviceType,
                "GetHostNumberOfEntries",
                controlUrl,
                null,
                cancellationToken);

            string countValue = FindFirstValue(countDoc, "NewHostNumberOfEntries") ?? "0";
            int count = ParseInt32Safe(countValue);
            int take = Math.Min(Math.Max(count, 0), Math.Max(maxHosts, 0));

            List<FritzHostEntry> hosts = new List<FritzHostEntry>(take);

            for (int i = 0; i < take; i++)
            {
                Dictionary<string, string> args = new Dictionary<string, string>(StringComparer.Ordinal);
                args["NewIndex"] = i.ToString();

                XDocument hostDoc = await this.CallSoapAsync(
                    serviceType,
                    "GetGenericHostEntry",
                    controlUrl,
                    args,
                    cancellationToken);

                string? mac = FindFirstValue(hostDoc, "NewMACAddress");
                string? ip = FindFirstValue(hostDoc, "NewIPAddress");
                string? name = FindFirstValue(hostDoc, "NewHostName").Replace ("Lasne", "");
                string activeText = FindFirstValue(hostDoc, "NewActive") ?? "0";
                string interfaceType = FindFirstValue(hostDoc, "NewInterfaceType") ?? String.Empty;
                string leaseTimeText = FindFirstValue(hostDoc, "NewLeaseTimeRemaining") ?? "0";

                bool active = StringComparer.Ordinal.Equals(activeText.Trim(), "1");
                int leaseTime = ParseInt32Safe(leaseTimeText);

                FritzHostEntry entry = new FritzHostEntry(
                    i,
                    name,
                    ip,
                    mac,
                    active,
                    interfaceType,
                    leaseTime);

                hosts.Add(entry);
            }

            return hosts;
        }

        public async Task<FritzDslInfo?> GetDslInfoAsync(CancellationToken cancellationToken = default)
        {
            string serviceType = "urn:dslforum-org:service:WANDSLInterfaceConfig:1";
            string action = "GetInfo";
            string controlUrl = "/upnp/control/wandslifconfig1";

            try
            {
                XDocument doc = await this.CallSoapAsync(
                    serviceType,
                    action,
                    controlUrl,
                    null,
                    cancellationToken);

                string downstreamCurrRateText = FindFirstValue(doc, "NewDownstreamCurrRate") ?? "0";
                string upstreamCurrRateText = FindFirstValue(doc, "NewUpstreamCurrRate") ?? "0";
                string downstreamMaxRateText = FindFirstValue(doc, "NewDownstreamMaxRate") ?? "0";
                string upstreamMaxRateText = FindFirstValue(doc, "NewUpstreamMaxRate") ?? "0";
                string downstreamNoiseMarginText = FindFirstValue(doc, "NewDownstreamNoiseMargin") ?? "0";
                string upstreamNoiseMarginText = FindFirstValue(doc, "NewUpstreamNoiseMargin") ?? "0";

                int downstreamCurrRate = ParseInt32Safe(downstreamCurrRateText);
                int upstreamCurrRate = ParseInt32Safe(upstreamCurrRateText);
                int downstreamMaxRate = ParseInt32Safe(downstreamMaxRateText);
                int upstreamMaxRate = ParseInt32Safe(upstreamMaxRateText);
                int downstreamNoiseMargin = ParseInt32Safe(downstreamNoiseMarginText);
                int upstreamNoiseMargin = ParseInt32Safe(upstreamNoiseMarginText);

                FritzDslInfo info = new FritzDslInfo(
                    downstreamCurrRate,
                    upstreamCurrRate,
                    downstreamMaxRate,
                    upstreamMaxRate,
                    downstreamNoiseMargin,
                    upstreamNoiseMargin);

                return info;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception)
            {
                // Often HTTP 500 with TR-064 fault when WANDSLInterfaceConfig is not usable.
                return null;
            }
        }

        public async Task<FritzTimeInfo?> GetTimeInfoAsync(CancellationToken cancellationToken = default)
        {
            string serviceType = "urn:dslforum-org:service:Time:1";
            string action = "GetInfo";
            string controlUrl = "/upnp/control/time";

            try
            {
                XDocument doc = await this.CallSoapAsync(
                    serviceType,
                    action,
                    controlUrl,
                    null,
                    cancellationToken);

                string ntpServer1 = FindFirstValue(doc, "NewNTPServer1") ?? string.Empty;
                string ntpServer2 = FindFirstValue(doc, "NewNTPServer2") ?? string.Empty;

                string? rawTime = FindFirstValue(doc, "NewCurrentLocalTime");
                DateTime? currentLocalTime = ParseDateTimeSafe(rawTime);

                // Per AVM spec these are "not supported" but many devices return them anyway.
                string? localTimeZone = FindFirstValue(doc, "NewLocalTimeZone");
                string? localTimeZoneName = FindFirstValue(doc, "NewLocalTimeZoneName");

                string? daylightRaw = FindFirstValue(doc, "NewDaylightSavingsUsed");
                bool? daylightSavingsUsed = daylightRaw is not null
                    ? StringComparer.OrdinalIgnoreCase.Equals(daylightRaw.Trim(), "1")
                        || StringComparer.OrdinalIgnoreCase.Equals(daylightRaw.Trim(), "true")
                    : null;

                return new FritzTimeInfo(
                    ntpServer1,
                    ntpServer2,
                    currentLocalTime,
                    string.IsNullOrWhiteSpace(localTimeZone) ? null : localTimeZone,
                    string.IsNullOrWhiteSpace(localTimeZoneName) ? null : localTimeZoneName,
                    daylightSavingsUsed);
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception)
            {
                // HTTP 500 SOAP fault can occur in bridge mode or if the service is unavailable.
                return null;
            }
        }

        public async Task<FritzSetNtpResult> SetNtpServerAsync(string ntpServer1, string ntpServer2, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ntpServer1) || string.IsNullOrWhiteSpace(ntpServer2))
            {
                return FritzSetNtpResult.Fail("NTP server address must not be empty.");
            }

            string serviceType = "urn:dslforum-org:service:Time:1";
            string controlUrl = "/upnp/control/time";

            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>(StringComparer.Ordinal);
                args["NewNTPServer1"] = ntpServer1;
                args["NewNTPServer2"] = ntpServer2;

                await this.CallSoapAsync(
                    serviceType, "SetNTPServers", controlUrl, args, cancellationToken);

                return FritzSetNtpResult.Ok();
            }
            catch (HttpRequestException ex)
            {
                return FritzSetNtpResult.Fail($"HTTP error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Now catches SOAP faults — including UPnP error 606 (Action not authorized)
                return FritzSetNtpResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return FritzSetNtpResult.Fail($"Unexpected error: {ex.Message}");
            }
        }

        private async Task<XDocument> CallSoapAsync(string serviceType, string action, string controlUrl, IReadOnlyDictionary<string, string>? inArguments, CancellationToken cancellationToken)
        {
            Uri controlUri = new Uri(this.baseUri, controlUrl);
            string soapBody = BuildSoapEnvelope(serviceType, action, inArguments);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, controlUri);
            request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");
            request.Headers.TryAddWithoutValidation("SOAPAction", serviceType + "#" + action);

            HttpResponseMessage response = await this.httpClient.SendAsync(request, cancellationToken);
            string responseXml = await response.Content.ReadAsStringAsync(cancellationToken);

            // Check HTTP error first (catches 401, 500, etc.)
            response.EnsureSuccessStatusCode();

            XDocument doc = XDocument.Parse(responseXml);

            // Additionally detect SOAP faults that arrive with HTTP 200
            ThrowIfSoapFault(doc, action);

            return doc;
        }

        private static void ThrowIfSoapFault(XDocument doc, string action)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XElement? fault = doc
                .Descendants(soapEnv + "Fault")
                .FirstOrDefault();

            if (fault is null)
            {
                return;
            }

            string faultCode = fault.Element("faultcode")?.Value ?? "unknown";
            string faultString = fault.Element("faultstring")?.Value ?? "unknown";

            // UPnP error detail (e.g. errorCode 606 = Action not authorized)
            XNamespace upnp = "urn:schemas-upnp-org:control-1-0";
            string? upnpErrorCode = fault
                .Descendants(upnp + "errorCode")
                .FirstOrDefault()?.Value;

            string? upnpErrorDesc = fault
                .Descendants(upnp + "errorDescription")
                .FirstOrDefault()?.Value;

            string message = $"SOAP fault in action '{action}': [{faultCode}] {faultString}";

            if (upnpErrorCode is not null)
            {
                message += $" | UPnP error {upnpErrorCode}: {upnpErrorDesc}";
            }

            throw new InvalidOperationException(message);
        }

        private static string BuildSoapEnvelope(string serviceType, string action, IReadOnlyDictionary<string, string>? inArguments)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            sb.Append(@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" ");
            sb.Append(@"s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">");
            sb.Append(@"<s:Body>");
            sb.Append(@"<u:" + action + @" xmlns:u=""" + serviceType + @""">");

            if (inArguments != null)
            {
                foreach (KeyValuePair<string, string> kvp in inArguments)
                {
                    sb.Append("<" + kvp.Key + ">");
                    sb.Append(System.Security.SecurityElement.Escape(kvp.Value));
                    sb.Append("</" + kvp.Key + ">");
                }
            }

            sb.Append(@"</u:" + action + @">");
            sb.Append(@"</s:Body>");
            sb.Append(@"</s:Envelope>");
            return sb.ToString();
        }

        private static string? FindFirstValue(XDocument doc, string localName)
        {
            XElement? el = doc
                .Descendants()
                .Where(e => e.Name.LocalName == localName)
                .FirstOrDefault();

            return el?.Value;
        }

        private static int ParseInt32Safe(string text)
        {
            if (int.TryParse(text.Trim(), out int value))
            {
                return value;
            }

            return 0;
        }

        private static DateTime? ParseDateTimeSafe(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // TR-064 DateTime format follows ISO 8601: "2026-03-08T13:41:00"
            if (DateTime.TryParse(
                    text.Trim(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime result))
            {
                return result;
            }

            return null;
        }
    }
}
