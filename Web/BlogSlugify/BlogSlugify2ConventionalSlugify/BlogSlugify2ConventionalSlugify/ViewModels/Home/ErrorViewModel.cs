namespace BlogSlugify2ConventionalSlugify.ViewModels.Home
{
    public class ErrorViewModel : BaseViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OriginalPath { get; set; } // The path that caused the error
        public string? ExceptionType { get; set; } // Type of the exception if one occurred
        public string? StackTrace { get; set; } // Stack trace, only for development
        public bool IsDevelopment { get; set; } // Flag to indicate if running in Development environment
    }
}
