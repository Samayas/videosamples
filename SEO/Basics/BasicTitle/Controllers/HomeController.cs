using System.Diagnostics;
using BasicTitle.ViewModels;
using BasicTitle.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace BasicTitle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel("The SEO Product page ever"));
        }

        public IActionResult Privacy()
        {
            return View(new HomeViewModel("The Privacy Policy page"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
