namespace MVC404Step1.ViewModels.Error
{
    public class ErrorViewModel : BaseViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
