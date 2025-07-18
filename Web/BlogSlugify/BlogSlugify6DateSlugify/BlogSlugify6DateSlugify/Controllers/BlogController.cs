using BlogSlugify6DateSlugify.Models.Blog;
using BlogSlugify6DateSlugify.Processors.Interfaces;
using BlogSlugify6DateSlugify.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;

namespace BlogSlugify6DateSlugify.Controllers
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

        public IActionResult Article(int year, int month, string article)
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            BlogPostModel blogPostModel = (from blogPost in blogPostModels where blogPost.FriendlyUrl == article  && blogPost.CreationDate.Year == year && blogPost.CreationDate.Month == month select blogPost).SingleOrDefault();
            if (blogPostModel == null) 
            {
                return RedirectToAction("Index");
            }

            return View(new BlogArticleViewModel() { Post = blogPostModel });
        }
    }
}
