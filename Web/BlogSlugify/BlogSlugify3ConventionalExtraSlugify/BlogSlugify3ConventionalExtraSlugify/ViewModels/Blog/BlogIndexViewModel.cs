using BlogSlugify3ConventionalExtraSlugify.Models.Blog;

namespace BlogSlugify3ConventionalExtraSlugify.ViewModels.Blog
{
    public class BlogIndexViewModel : BaseViewModel
    {
        public IList<BlogPostModel> Posts { get; set; } = new List<BlogPostModel>();
    }
}
