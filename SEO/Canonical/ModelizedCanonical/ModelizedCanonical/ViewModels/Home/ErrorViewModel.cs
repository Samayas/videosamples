namespace ModelizedCanonical.ViewModels.Home
{
    public class ErrorViewModel : BaseViewModel
    {
        public ErrorViewModel() : base("", "", "", null)
        {
        }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
