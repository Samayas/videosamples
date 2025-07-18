using BlogSlugify2ConventionalSlugify.Models.Blog;

namespace BlogSlugify2ConventionalSlugify.ViewModels.Blog
{
    public class BlogIndexViewModel : BaseViewModel
    {
        public IList<BlogPostModel> Posts { get; set; } = new List<BlogPostModel>();
    }
}
