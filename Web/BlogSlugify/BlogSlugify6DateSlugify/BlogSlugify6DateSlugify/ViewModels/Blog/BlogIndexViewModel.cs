using BlogSlugify6DateSlugify.Models.Blog;

namespace BlogSlugify6DateSlugify.ViewModels.Blog
{
    public class BlogIndexViewModel : BaseViewModel
    {
        public IList<BlogPostModel> Posts { get; set; }
    }
}
