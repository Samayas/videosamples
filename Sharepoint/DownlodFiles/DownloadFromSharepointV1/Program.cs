using Microsoft.Extensions.Configuration;
using DownloadFiles.DownloadFromSharepointV1.Services;
using DownloadFiles.DownloadFromSharepointV1.Settings;

namespace DownloadFiles.DownloadFromSharepointV1
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

            Console.WriteLine("Reading and bind Settings");
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
            await sharePointUploaderService.UploadWithMetadataAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\1MB.file", new Dictionary<string, object> { { "User", "Me" }, { "Confidential", "Public" } }, "App1");

            Console.WriteLine("Upload File 2MB");
            await sharePointUploaderService.UploadWithMetadataAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\2MB.file", new Dictionary<string, object> { { "User", "You" }, { "Confidential", "Confidential" } }, "App1");

            Console.WriteLine("Upload File 4MB");
            await sharePointUploaderService.UploadWithMetadataAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\4MB.file", new Dictionary<string, object> { { "User", "Them" }, { "Confidential", "Public" } }, "App2");

            Console.WriteLine("Upload File 8MB");
            await sharePointUploaderService.UploadWithMetadataAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\8MB.file", new Dictionary<string, object> { { "User", "Her" }, { "Confidential", "Confidential" } }, "App2");

            Console.WriteLine("Upload File 12MB");
            await sharePointUploaderService.UploadWithMetadataAsync(driveId, "D:\\Projects\\Github\\VideoSamples\\Sharepoint\\UploadFiles\\CreateFiles\\bin\\Debug\\net10.0\\12MB.file", new Dictionary<string, object> { { "User", "Him" }, { "Confidential", "Public" } }, "App2");
        }
    }
}
