using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using PDUManagement.LogiLinkServiceAgent.Interfaces;
using PDUManagement.PDUBaseServiceAgent;

namespace PDUManagement.LogiLinkServiceAgent
{
    public class LogiLinkServiceAgent : ILogiLinkServiceAgent
    {
        const string statusPage = "http://{0}/status.xml";
        const string socketPage = "http://{0}/control_outlet.htm";
        const string socketSwitchPage = "http://{0}/control_outlet.htm?{1}=1&op={2}&submit=Apply";

        private PDUDetails pduDetails;

        public LogiLinkServiceAgent(PDUDetails pduDetails)
        {
            this.pduDetails = pduDetails;
        }

        public async Task<string> GetPDULoadAsync()
        {
            string load = await DownloadStatusPage(PDUStatusXmlEnum.Load);

            return load;
        }

        public async Task<string> GetPDUTemperatureAsync()
        {
            string temperature = await DownloadStatusPage(PDUStatusXmlEnum.Temperature);

            return temperature;
        }

        public async Task<string> GetPDUHumidityAsync()
        {
            string humidity = await DownloadStatusPage(PDUStatusXmlEnum.Humidity);

            return humidity;
        }

        public async Task<string> GetPDUStateAsync()
        {
            string status = await DownloadStatusPage(PDUStatusXmlEnum.Status);

            return status;
        }

        public async Task<IDictionary<string, string>> GetSocketsAsync()
        {
            // Set Correct Url
            string socketUrl = string.Format(socketPage, this.pduDetails.PDUServerAddress);

            // Download Page
            string htmlResult = await DownloadLogiLinkPage(socketUrl);

            // Parse Html Page
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlResult);

            IDictionary<string, string> socketsDictionary = new Dictionary<string, string>();

            // Find Table
            HtmlNode? table = htmlDocument.DocumentNode.SelectSingleNode("//table");
            if (table != null)
            {
                HtmlNodeCollection trs = table.SelectNodes("//tr");
                foreach (HtmlNode tr in trs)
                {
                    Tuple<string, string> socketIdAndLabel = FindSocketIdAndLabelFromTrRecord(tr);
                    if (socketIdAndLabel != null)
                    {
                        socketsDictionary.Add(socketIdAndLabel.Item1, socketIdAndLabel.Item2);
                    }  
                }
            }

            // Default Result
            return await Task.FromResult(socketsDictionary);
        }

        public async Task<IDictionary<string, SocketStateEnum>> GetSocketsAndStateAsync()
        {
            // Socket List
            IDictionary<String, string> socketList = await GetSocketsAsync();

            // Set Correct Url
            string statusUrl = string.Format(statusPage, this.pduDetails.PDUServerAddress);

            // Download Page
            string htmlResult = await DownloadLogiLinkPage(statusUrl);

            // Parse Html Page
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlResult);

            // Parse Xml Result
            XmlDocument statusDocument = new XmlDocument();
            statusDocument.LoadXml(htmlResult);

            IDictionary<string, SocketStateEnum> socketsDictionary = new Dictionary<string, SocketStateEnum>();

            foreach (KeyValuePair<string, string> socketId in socketList)
            {
                XmlNode statusNode = statusDocument.SelectSingleNode($"/response/{socketId.Key}");

                socketsDictionary.Add(socketId.Value, GetSocketStateFromString(statusNode));
            }

            // Default Result
            return await Task.FromResult(socketsDictionary);
        }

        public async Task SwitchSocketAsync(string socketName, SocketStateEnum newSocketState)
        {
            // Socket List
            IDictionary<String, string> socketList = await GetSocketsAsync();

            string socketId = string.Empty;
            foreach (KeyValuePair<string, string> socket in socketList) 
            {
                if (socket.Value == socketName)
                {
                    socketId = socket.Key;
                    break;
                }
            }

            if (socketId == string.Empty)
            {
                return;
            }

            string socketTechnicalId = socketId.Replace("Stat", "");

            // Build url
            string socketSwitchPageUrl = string.Format(socketSwitchPage, this.pduDetails.PDUServerAddress, socketTechnicalId, newSocketState == SocketStateEnum.On ? 0 : 1);

            await DownloadLogiLinkPage(socketSwitchPageUrl);
        }

        public async Task RecycleSocketAsync(string socketName)
        {
            await SwitchSocketAsync(socketName, SocketStateEnum.Off);
            await SwitchSocketAsync(socketName, SocketStateEnum.On);
        }

        private async Task<string> DownloadStatusPage(PDUStatusXmlEnum pduStatusXmlEnum)
        {
            // Set Correct Url & XMl PDU Status Key
            string statusUrl = string.Format(statusPage, this.pduDetails.PDUServerAddress);
            string pduStatus = ConvertPDUStatusXmlEnumToString(pduStatusXmlEnum);

            // Download Page
            string htmlResult = await DownloadLogiLinkPage(statusUrl);

            // Parse Xml Result
            XmlDocument statusDocument = new XmlDocument();
            statusDocument.LoadXml(htmlResult);

            // Find Element and return
            XmlNode statusNode = statusDocument.SelectSingleNode($"/response/{pduStatus}");
            if (statusNode != null )
            {
                return statusNode.InnerText;
            }

            // Default Result
            return await Task.FromResult(string.Empty);
        }

        private async Task<string> DownloadLogiLinkPage(string url)
        {
            // Build Basic Auth string
            var username = this.pduDetails.UserName;
            var password = this.pduDetails.Password; ;
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            var base64Credentials = Convert.ToBase64String(byteArray);

            // Download Url
            using (HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
            {
                // Set Basic Auth
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

                // Retrieve Result
                HttpResponseMessage authResponse = await client.GetAsync(url);
                if (authResponse.StatusCode == HttpStatusCode.OK)
                {
                    return await authResponse.Content.ReadAsStringAsync();
                }
            }

            return string.Empty;
        }

        private static SocketStateEnum GetSocketStateFromString(XmlNode statusNode)
        {
            switch (statusNode.InnerText) 
            {
                case "off":
                    return SocketStateEnum.Off;
                case "on":
                    return SocketStateEnum.On;
            }

            return SocketStateEnum.Unknown;
        }

        private Tuple<string, string> FindSocketIdAndLabelFromTrRecord(HtmlNode trRecord)
        {
            HtmlNode span = trRecord.SelectSingleNode("td[2]/span");
            if (span == null)
            {
                return null;
            }

            string socketId = span.Id;

            HtmlNode td = trRecord.SelectSingleNode("td[1]");
            string label = td.InnerHtml;

            return new Tuple<string, string>(socketId, label);
        }

        private string ConvertPDUStatusXmlEnumToString(PDUStatusXmlEnum pduStatusXmlEnum)
        {
            switch (pduStatusXmlEnum)
            {
                case PDUStatusXmlEnum.Load:
                    return "cur0";
                case PDUStatusXmlEnum.Temperature:
                    return "tempBan";
                case PDUStatusXmlEnum.Humidity:
                    return "humBan";
                case PDUStatusXmlEnum.Status:
                    return "statBan";
                case PDUStatusXmlEnum.Socket1:
                    return "outletStat0";
                case PDUStatusXmlEnum.Socket2:
                    return "outletStat1";
                case PDUStatusXmlEnum.Socket3:
                    return "outletStat2";
                case PDUStatusXmlEnum.Socket4:
                    return "outletStat3";
                case PDUStatusXmlEnum.Socket5:
                    return "outletStat4";
                case PDUStatusXmlEnum.Socket6:
                    return "outletStat5";
                case PDUStatusXmlEnum.Socket7:
                    return "outletStat6";
                case PDUStatusXmlEnum.Socket8:
                    return "outletStat7";
            }

            return string.Empty;
        }
    }
}
