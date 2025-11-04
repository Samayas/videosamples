namespace BasicMetaDescription.ViewModels
{
    public abstract class BaseViewModel
    {
        public BaseViewModel(string title, string metaDescription)
        {
            this.Title = title;
            this.MetaDescription = metaDescription;
        }

        public string Title { get; set; }
        public string MetaDescription { get; set; }
    }
}
