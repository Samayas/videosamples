using  UploadAndScan2Full.Models;

namespace UploadAndScan2Full.Services
{
    public interface IClamAvScannerService
    {
        Task<ScanResult> ScanFileAsync(string filePath);
    }
}
