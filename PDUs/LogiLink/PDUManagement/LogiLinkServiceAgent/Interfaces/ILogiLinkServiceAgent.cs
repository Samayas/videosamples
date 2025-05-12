using PDUManagement.PDUBaseServiceAgent.Interfaces;

namespace PDUManagement.LogiLinkServiceAgent.Interfaces
{
    public interface ILogiLinkServiceAgent : IPDUServiceAgent
    {
        Task<string> GetPDULoadAsync();
        Task<string> GetPDUTemperatureAsync();
        Task<string> GetPDUHumidityAsync();
        Task<string> GetPDUStateAsync();
    }
}
