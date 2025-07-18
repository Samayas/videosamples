using BlogSlugify1Init.Models.Blog;

namespace BlogSlugify1Init.Processors.Interfaces
{
    public interface IBlogProcessor
    {
        IList<BlogPostModel> GenerateBlogsListing();
    }
}
