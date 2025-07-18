using BlogSlugify6DateSlugify.MVC.Page;

namespace BlogSlugify6DateSlugify.Models.Blog
{
    public class BlogPostModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string FriendlyUrl { get; set; } = string.Empty;

        public string Post { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; }

        public static string MakeFriendlyUrl(string url)
        {
            return FriendlyUrlHelper.GetFriendlyTitle(url, true);
        }
    }
}
