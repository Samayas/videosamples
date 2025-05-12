using PDUManagement.PDUBaseServiceAgent;

namespace AdminManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public IDictionary<string, SocketStateEnum> SocketsAndState { get; set; } = new Dictionary<string, SocketStateEnum>();

        public string Load { get; set; } = string.Empty;

        public string Temperature { get; set; } = string.Empty;

        public string Humidity { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
    }
}
