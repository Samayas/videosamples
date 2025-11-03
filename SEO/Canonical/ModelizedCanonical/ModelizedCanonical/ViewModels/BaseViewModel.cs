using ModelizedCanonical.Models;

namespace ModelizedCanonical.ViewModels
{
    public abstract class BaseViewModel
    {
        public BaseViewModel(string title, string metaDescription, string keywords, CanonicalModel? canonicalModel)
        {
            this.Title = title;
            this.MetaDescription = metaDescription;
            this.Keywords = keywords;
            this.CanonicalModel = canonicalModel;
        }

        public string Title { get; set; }
        public string MetaDescription { get; set; }
        public string Keywords { get; set; }

        public CanonicalModel? CanonicalModel { get; set; }
    }
}
