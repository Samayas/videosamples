namespace PDUManagement.PDUBaseServiceAgent.Interfaces
{
    public interface IPDUServiceAgent
    {
        Task<IDictionary<string, string>> GetSocketsAsync();
        Task<IDictionary<string, SocketStateEnum>> GetSocketsAndStateAsync();
        Task SwitchSocketAsync(string socketName, SocketStateEnum newSocketState);
        Task RecycleSocketAsync(string socketName);
    }
}
