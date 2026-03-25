namespace TR064.Model
{
    public class FritzSetNtpResult
    {
        private FritzSetNtpResult(bool success, string? errorMessage)
        {
            this.Success = success;
            this.ErrorMessage = errorMessage;
        }

        public bool Success { get; }
        public string? ErrorMessage { get; }

        public static FritzSetNtpResult Ok() => new FritzSetNtpResult(true, null);

        public static FritzSetNtpResult Fail(string message) => new FritzSetNtpResult(false, message);
    }
}
