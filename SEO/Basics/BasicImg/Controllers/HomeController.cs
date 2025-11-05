using System.Diagnostics;
using BasicHeadings.ViewModels;
using BasicHeadings.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace Basicd.Controllers
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
            return View(new HomeViewModel("The SEO Product page ever", "This page describes how to design your best SEO pages", "SEO;Home;Compay"));
        }

        public IActionResult Privacy()
        {
            return View(new HomeViewModel("The Privacy Policy page", "This Privacy page describes how to design your best SEO pages", "Privacy;SEO;Home;Compay"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
