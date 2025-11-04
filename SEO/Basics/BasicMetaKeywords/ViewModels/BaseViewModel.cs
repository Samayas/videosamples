namespace BasicMetaKeywords.ViewModels
{
    public abstract class BaseViewModel
    {
        public BaseViewModel(string title, string metaDescription, string keywords)
        {
            this.Title = title;
            this.MetaDescription = metaDescription;
            this.Keywords = keywords;
        }

        public string Title { get; set; }
        public string MetaDescription { get; set; }
        public string Keywords { get; set; }
    }
}
