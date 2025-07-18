using BlogSlugify3ConventionalExtraSlugify.MVC.Page;

namespace BlogSlugify3ConventionalExtraSlugify.Models.Blog
{
    public class BlogPostModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Post { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; }

        public static string MakeFriendlyUrl(string url)
        {
            return FriendlyUrlHelper.GetFriendlyTitle(url, true);
        }
    }
}
