namespace BlogSlugify1Init.Models.Blog
{
    public class BlogPostModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Post { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; }
    }
}
