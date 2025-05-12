using System.Diagnostics;
using AdminManagement.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using PDUManagement.LogiLinkServiceAgent.Interfaces;
using PDUManagement.Models;
using PDUManagement.PDUBaseServiceAgent;
using PDUManagement.PDUBaseServiceAgent.Interfaces;

namespace PDUManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IPDUServiceAgent pduServiceAgent;

        public HomeController(ILogger<HomeController> logger, IPDUServiceAgent pduServiceAgent)
        {
            this.logger = logger;
            this.pduServiceAgent = pduServiceAgent;
        }

        public async Task<IActionResult> Index()
        {
            IDictionary<string, SocketStateEnum> socketsAndState = await this.pduServiceAgent.GetSocketsAndStateAsync();

            string load = string.Empty;
            string temperature = string.Empty;
            string humidity = string.Empty;
            string status = string.Empty;

            // Cast Service Agent to Specific
            if (this.pduServiceAgent is ILogiLinkServiceAgent)
            {
                ILogiLinkServiceAgent logiLinkServiceAgent =(ILogiLinkServiceAgent) this.pduServiceAgent;

                load = await logiLinkServiceAgent.GetPDULoadAsync();
                temperature = await logiLinkServiceAgent.GetPDUTemperatureAsync();
                humidity = await logiLinkServiceAgent.GetPDUHumidityAsync();
                status = await logiLinkServiceAgent.GetPDUStateAsync();
            }

            return View(new IndexViewModel() { SocketsAndState = socketsAndState, Load = load, Temperature = temperature, Humidity = humidity, Status = status });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSocket(string socketName, string socketValue)
        {
            SocketStateEnum socketStateEnum = socketValue == "On" ? SocketStateEnum.Off : SocketStateEnum.On;

            await this.pduServiceAgent.SwitchSocketAsync(socketName, socketStateEnum);

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
