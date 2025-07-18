using BlogSlugify2ConventionalSlugify.Models.Blog;
using BlogSlugify2ConventionalSlugify.Processors.Interfaces;
using BlogSlugify2ConventionalSlugify.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;

namespace BlogSlugify2ConventionalSlugify.Controllers
{
    public class BlogController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IBlogProcessor blogProcessor;

        public BlogController(ILogger<HomeController> logger, IBlogProcessor blogProcessor)
        {
            this.logger = logger;
            this.blogProcessor = blogProcessor;
        }

        public IActionResult Index()
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            return View(new BlogIndexViewModel() { Posts = blogPostModels });
        }

        public IActionResult SiteIndex()
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            return View(new BlogSiteIndexViewModel() { Count = blogPostModels.Count });
        }

        public IActionResult Article(string article)
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            BlogPostModel blogPostModel = (from blogPost in blogPostModels where blogPost.Slug == article select blogPost).SingleOrDefault();
            if (blogPostModel == null) 
            {
                return RedirectToAction("Index");
            }

            return View(new BlogArticleViewModel() { Post = blogPostModel });
        }
    }
}
