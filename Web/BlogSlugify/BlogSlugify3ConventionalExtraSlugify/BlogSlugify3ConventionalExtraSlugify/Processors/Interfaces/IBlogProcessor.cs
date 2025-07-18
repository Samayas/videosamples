using BlogSlugify3ConventionalExtraSlugify.Models.Blog;

namespace BlogSlugify3ConventionalExtraSlugify.Processors.Interfaces
{
    public interface IBlogProcessor
    {
        IList<BlogPostModel> GenerateBlogsListing();

    }
}
