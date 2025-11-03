using ModelizedCanonical.Models;

namespace ModelizedCanonical.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(string title, string metaDescription, string keywords, CanonicalModel canonicalModel) : base(title, metaDescription, keywords, canonicalModel) 
        {
        }
    }
}
