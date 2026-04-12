using Microsoft.Extensions.Configuration;
using UploadFiles.UploadToSharepointV1.Services;
using UploadFiles.UploadToSharepointV1.Settings;

namespace UploadFiles.UploadToSharepointV1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Load Configuration");
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            Console.WriteLine("Reading Settings");
            SharePointSettings sharePointSettings = new SharePointSettings();
            config.GetSection("SharePoint").Bind(sharePointSettings);

            SharePointUploaderService sharePointUploaderService = new SharePointUploaderService(sharePointSettings);

            Console.WriteLine("Retrieve SiteId");
            string siteId = await sharePointUploaderService.GetSiteIdAsync("samayas.sharepoint.com", "/sites/UploadFiles");
            Console.WriteLine($"SiteId : {siteId}");

            Console.WriteLine("Retrieve DriveId");
            string driveId = await sharePointUploaderService.GetDriveIdAsync(siteId, "Documents");
            Console.WriteLine($"DriveId : {driveId}");

            Console.WriteLine("Upload File 1MB");
            await sharePointUploaderService.UploadFileAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\1MB.file", "App1");

            Console.WriteLine("Upload File 2MB");
            await sharePointUploaderService.UploadFileAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\2MB.file", "App1");

            Console.WriteLine("Upload File 4MB");
            await sharePointUploaderService.UploadFileAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\4MB.file", "App2");

            Console.WriteLine("Upload File 8MB");
            await sharePointUploaderService.UploadFileAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\8MB.file", "App2");

            Console.WriteLine("Upload File 12MB");
            await sharePointUploaderService.UploadFileAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\12MB.file", "App2");
        }
    }
}
