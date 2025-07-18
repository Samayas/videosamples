using BlogSlugify2ConventionalSlugify.Models.Blog;

namespace BlogSlugify2ConventionalSlugify.Processors.Interfaces
{
    public interface IBlogProcessor
    {
        IList<BlogPostModel> GenerateBlogsListing();

    }
}
