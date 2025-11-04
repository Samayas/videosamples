namespace BasicMetaKeywords.ViewModels.Home
{
    public class ErrorViewModel : BaseViewModel
    {
        public ErrorViewModel(): base("", "", "")
        {
        }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
