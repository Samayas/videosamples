using System.Diagnostics;
using SimpleJSonLD.ViewModels;
using SimpleJSonLD.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace SimpleJSonLDd.Controllers
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
            return View(new HomeViewModel("The Privacy Policy page", "This page describes how to design your best SEO pages", "SEO;Home;Compay;Privacy"));
        }

        public IActionResult Article()
        {
            return View(new HomeViewModel("Article 1 page", "an article about Samayas", "SEO;Home;Compay;Article"));
        }

        public IActionResult BlogPost()
        {
            return View(new HomeViewModel("Blog Post page", "This blog post show your blog", "SEO;Home;Compay;Blogpost"));
        }

        public IActionResult About()
        {
            return View(new HomeViewModel("About", "This about page describes your company", "SEO;Home;Compay;About"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
