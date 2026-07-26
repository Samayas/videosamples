using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC404Step1.ViewModels.Home;

namespace MVC404Step1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IWebHostEnvironment environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            this.logger = logger;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }
    }
}
