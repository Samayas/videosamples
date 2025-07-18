using BlogSlugify4RouteSlugify.Models.Blog;
using BlogSlugify4RouteSlugify.Processors.Interfaces;
using BlogSlugify4RouteSlugify.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;

namespace BlogSlugify4RouteSlugify.Controllers
{
    [Route("[controller]")]
    public class BlogController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IBlogProcessor blogProcessor;

        public BlogController(ILogger<HomeController> logger, IBlogProcessor blogProcessor)
        {
            this.logger = logger;
            this.blogProcessor = blogProcessor;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            return View(new BlogIndexViewModel() { Posts = blogPostModels });
        }

        [HttpGet("siteindex")]
        public IActionResult SiteIndex()
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            return View(new BlogSiteIndexViewModel() { Count = blogPostModels.Count });
        }

        [HttpGet("{slug}")]
        public IActionResult Article(string slug)
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            BlogPostModel blogPostModel = (from blogPost in blogPostModels where blogPost.slug == slug select blogPost).SingleOrDefault();
            if (blogPostModel == null) 
            {
                return RedirectToAction("Index");
            }

            return View(new BlogArticleViewModel() { Post = blogPostModel });
        }
    }
}
