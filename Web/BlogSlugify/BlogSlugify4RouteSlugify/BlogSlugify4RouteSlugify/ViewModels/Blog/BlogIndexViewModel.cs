using BlogSlugify4RouteSlugify.Models.Blog;

namespace BlogSlugify4RouteSlugify.ViewModels.Blog
{
    public class BlogIndexViewModel : BaseViewModel
    {
        public IList<BlogPostModel> Posts { get; set; } = new List<BlogPostModel>();
    }
}
