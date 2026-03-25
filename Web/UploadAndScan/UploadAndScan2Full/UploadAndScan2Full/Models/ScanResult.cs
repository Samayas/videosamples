namespace UploadAndScan2Full.Models
{
    public class ScanResult
    {
        public bool IsInfected { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ScanDetails { get; set; } = string.Empty;
        public DateTime ScanDate { get; set; }
    }
}
