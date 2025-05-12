using AdminManagement.Contracts.Response;
using Microsoft.AspNetCore.Mvc;
using PDUManagement.PDUBaseServiceAgent;
using PDUManagement.PDUBaseServiceAgent.Interfaces;

namespace PDUManagement.Controllers
{
    public class ApiController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IPDUServiceAgent pduServiceAgent;

        public ApiController(ILogger<HomeController> logger, IPDUServiceAgent pduServiceAgent)
        {
            this.logger = logger;
            this.pduServiceAgent = pduServiceAgent;
        }

        [HttpGet("/api/socketstatus")]
        [Produces("application/json")]
        public async Task<ActionResult<SocketsStatus>> GetSocketStatus()
        {
            IDictionary<string, SocketStateEnum> socketsAndState = await this.pduServiceAgent.GetSocketsAndStateAsync();

            List<SocketStatus> sockets = new List<SocketStatus>();
            foreach (var socket in socketsAndState) 
            {
                sockets.Add(new SocketStatus() { SocketName = socket.Key, Status = socket.Value.ToString() });
            }

            return new SocketsStatus() { Sockets = sockets };
        }
    }
}
   