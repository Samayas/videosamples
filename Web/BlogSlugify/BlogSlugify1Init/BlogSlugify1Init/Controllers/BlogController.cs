using BlogSlugify1Init.Models.Blog;
using BlogSlugify1Init.Processors.Interfaces;
using BlogSlugify1Init.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;

namespace BlogSlugify1Init.Controllers
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

        public IActionResult Article(int id)
        {
            IList<BlogPostModel> blogPostModels = this.blogProcessor.GenerateBlogsListing();

            BlogPostModel blogPostModel = (from blogPost in blogPostModels where blogPost.Id == id select blogPost).SingleOrDefault();
            if (blogPostModel == null) 
            {
                return RedirectToAction("Index");
            }

            return View(new BlogArticleViewModel() { Post = blogPostModel });
        }
    }
}
