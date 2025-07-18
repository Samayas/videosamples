using BlogSlugify5RouteExtraSlugify.Models.Blog;

namespace BlogSlugify5RouteExtraSlugify.Processors.Interfaces
{
    public interface IBlogProcessor
    {
        IList<BlogPostModel> GenerateBlogsListing();

    }
}
