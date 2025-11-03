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
            CanonicalModel canonicalModel = new CanonicalModel(this.SiteUrl, $"/{this.ControllerShortName}/{this.GetActionName()}");
            JSonLDModel jsonLDModel = CreateJSonLDModel("", "This page describes how to design your best SEO pages", null, JSonLDType.WebPage);
           
            return View(new HomeViewModel("The SEO Product page ever", "This page describes how to design your best SEO pages", "SEO;Home;Compay", canonicalModel, jsonLDModel));
        }

        public IActionResult Privacy()
        {
            CanonicalModel canonicalModel = new CanonicalModel(this.SiteUrl, $"/{this.ControllerShortName}/{this.GetActionName()}");
            JSonLDModel jsonLDModel = CreateJSonLDModel("My Article", "This page describes how to design your best SEO pages", null, JSonLDType.WebPage);

            return View(new HomeViewModel("The Privacy Policy page", "This page describes how to design your best SEO pages", "SEO;Home;Compay;Privacy", canonicalModel, jsonLDModel));
        }

        public IActionResult Article()
        {
            CanonicalModel canonicalModel = new CanonicalModel(this.SiteUrl, $"/{this.ControllerShortName}/{this.GetActionName()}");
            JSonLDModel jsonLDModel = CreateJSonLDModel("My Post", "an article about Samayas", null, JSonLDType.Article);

            return View(new HomeViewModel("Article 1 page", "an article about Samayas", "SEO;Home;Compay;Article", canonicalModel, jsonLDModel));
        }

        public IActionResult BlogPost()
        {
            CanonicalModel canonicalModel = new CanonicalModel(this.SiteUrl, $"/{this.ControllerShortName}/{this.GetActionName()}");
            JSonLDModel jsonLDModel = CreateJSonLDModel("", "This blog post show your blog", null, JSonLDType.BlogPosting);

            return View(new HomeViewModel("Blog Post page", "This blog post show your blog", "SEO;Home;Compay;Blogpost", canonicalModel, jsonLDModel));
        }

        public IActionResult About()
        {
            CanonicalModel canonicalModel = new CanonicalModel(this.SiteUrl, $"/{this.ControllerShortName}/{this.GetActionName()}");
            JSonLDModel jsonLDModel = CreateJSonLDModel("", "This about page describes your company", null, JSonLDType.WebPage);

            return View(new HomeViewModel("About", "This about page describes your company", "SEO;Home;Compay;About", canonicalModel, jsonLDModel));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public string SiteUrl
        {
            get { return "https://localhost:7078"; }
        }

        private JSonLDModel CreateJSonLDModel(string headline, string description, string[]? images = null, JSonLDType jsonLDType = JSonLDType.WebPage)
        {
            JSonLDModel jsonLDModel = new JSonLDModel();

            jsonLDModel.Headline = headline;
            jsonLDModel.Description = description;
            jsonLDModel.HasJSonLD = true;
            jsonLDModel.Type = jsonLDType;
            jsonLDModel.Author = "Samayas";
            jsonLDModel.Publisher = "Samayas";
            jsonLDModel.PublisherOrganization = "Samayas";
            jsonLDModel.PublisherLogo = this.SiteUrl + "/images/logo/Samayas.jfif";
            jsonLDModel.PublisherLogoSize = "157,156";

            return jsonLDModel;
        }

    }
}
