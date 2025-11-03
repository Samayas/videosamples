using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModelizedCanonical.Models;
using ModelizedCanonical.ViewModels;
using ModelizedCanonical.ViewModels.Home;

namespace ModelizedCanonicald.Controllers
{
    public class HomeController : WebController<HomeController>
    {
        public HomeController(ILogger<HomeController> logger) : base(logger)
        {
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel("The SEO Product page ever", "This page describes how to design your best SEO pages", "SEO;Home;Compay",
                new CanonicalModel(this.SiteUrl, "/Home/Index")));
        }

        public IActionResult Privacy()
        {
            return View(new HomeViewModel("The Privacy Policy page", "This page describes how to design your best SEO pages", "SEO;Home;Compay;Privacy",
                new CanonicalModel(this.SiteUrl, $"/{this.ControllerShortName}/{this.GetActionName()}")));
        }

        public IActionResult Article()
        {
            return View(new HomeViewModel("Article 1 page", "an article about Samayas", "SEO;Home;Compay;Article", new CanonicalModel(this.SiteUrl, "/Home/Index")));
        }

        public IActionResult BlogPost()
        {
            return View(new HomeViewModel("Blog Post page", "This blog post show your blog", "SEO;Home;Compay;Blogpost", new CanonicalModel(this.SiteUrl, "/Home/Index")));
        }

        public IActionResult About()
        {
            return View(new HomeViewModel("About", "This about page describes your company", "SEO;Home;Compay;About", new CanonicalModel(this.SiteUrl, "/Home/Index")));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
