using System.Diagnostics;
using BasicMetaDescription.ViewModels;
using BasicMetaDescription.ViewModels.Home;
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
            return View(new HomeViewModel("The SEO Product page ever", "This page describes how to design your best SEO pages"));
        }

        public IActionResult Privacy()
        {
            return View(new HomeViewModel("The Privacy Policy page", "This privacy page describes how to design your best SEO pages"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
