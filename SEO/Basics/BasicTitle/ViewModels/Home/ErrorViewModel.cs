namespace BasicTitle.ViewModels.Home
{
    public class ErrorViewModel : BaseViewModel
    {
        public ErrorViewModel() : base("Error")
        {
        }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
