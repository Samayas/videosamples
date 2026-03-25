using nClam;
using  UploadAndScan2Full.Models;

namespace UploadAndScan2Full.Services
{
    public class ClamAvScannerService : IClamAvScannerService
    {
        private readonly IConfiguration configuration;

        public ClamAvScannerService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<ScanResult> ScanFileAsync(string filePath)
        {
            string? fileName = Path.GetFileName(filePath);
            try
            {
                string host = this.configuration["ClamAV:Host"] ?? "localhost";
                int port = int.Parse(this.configuration["ClamAV:Port"] ?? "3310");

                ClamClient clam = new ClamClient(host, port);
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                using MemoryStream stream = new MemoryStream(fileBytes);
                ClamScanResult scanResult = await clam.SendAndScanFileAsync(stream);

                return scanResult.Result switch
                {
                    ClamScanResults.Clean => new ScanResult
                    {
                        IsInfected = false,
                        FileName = fileName,
                        Message = "File scanned successfully - no threats detected",
                        ScanDetails = scanResult.RawResult,
                        ScanDate = DateTime.UtcNow
                    },
                    ClamScanResults.VirusDetected => new ScanResult
                    {
                        IsInfected = true,
                        FileName = fileName,
                        Message = $"Virus detected: {scanResult.InfectedFiles?.FirstOrDefault()?.VirusName}",
                        ScanDetails = scanResult.RawResult,
                        ScanDate = DateTime.UtcNow
                    },
                    _ => new ScanResult
                    {
                        IsInfected = true,
                        FileName = fileName,
                        Message = $"Scan error: {scanResult.RawResult}",
                        ScanDetails = scanResult.RawResult,
                        ScanDate = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return new ScanResult
                {
                    IsInfected = true,
                    FileName = fileName,
                    Message = $"Error during scan: {ex.Message}",
                    ScanDetails = ex.ToString(),
                    ScanDate = DateTime.UtcNow
                };
            }
        }
    }
}
