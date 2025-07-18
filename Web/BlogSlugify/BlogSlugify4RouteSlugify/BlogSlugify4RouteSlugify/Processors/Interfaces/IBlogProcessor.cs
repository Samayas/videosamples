using BlogSlugify4RouteSlugify.Models.Blog;

namespace BlogSlugify4RouteSlugify.Processors.Interfaces
{
    public interface IBlogProcessor
    {
        IList<BlogPostModel> GenerateBlogsListing();

    }
}
