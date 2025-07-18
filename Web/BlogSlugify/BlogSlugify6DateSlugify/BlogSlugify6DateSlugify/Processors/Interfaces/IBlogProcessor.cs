using BlogSlugify6DateSlugify.Models.Blog;

namespace BlogSlugify6DateSlugify.Processors.Interfaces
{
    public interface IBlogProcessor
    {
        IList<BlogPostModel> GenerateBlogsListing();

    }
}
