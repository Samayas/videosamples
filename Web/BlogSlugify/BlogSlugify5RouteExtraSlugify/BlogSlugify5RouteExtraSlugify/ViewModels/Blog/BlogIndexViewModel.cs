using BlogSlugify5RouteExtraSlugify.Models.Blog;

namespace BlogSlugify5RouteExtraSlugify.ViewModels.Blog
{
    public class BlogIndexViewModel : BaseViewModel
    {
        public IList<BlogPostModel> Posts { get; set; } = new List<BlogPostModel>();
    }
}
