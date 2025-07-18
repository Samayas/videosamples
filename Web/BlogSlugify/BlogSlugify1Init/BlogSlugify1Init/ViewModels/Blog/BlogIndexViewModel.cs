using BlogSlugify1Init.Models.Blog;

namespace BlogSlugify1Init.ViewModels.Blog
{
    public class BlogIndexViewModel : BaseViewModel
    {
        public IList<BlogPostModel> Posts { get; set; } = new List<BlogPostModel>();
    }
}
