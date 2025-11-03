using ModelizedCanonical.Models;

namespace ModelizedCanonical.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(string title, string metaDescription, string keywords, CanonicalModel? canonicalModel, JSonLDModel? jsonLDModel) : base(title, metaDescription, keywords, canonicalModel, jsonLDModel) 
        {
        }
    }
}
