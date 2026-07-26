#region Usings
using Microsoft.Extensions.Configuration;

using UploadFiles.UploadToSharepointV1.Services;
using UploadFiles.UploadToSharepointV1.Settings;
#endregion

namespace UploadFiles.UploadToSharepointV1
{
    /// <summary>
    /// Console sample that uploads small files to SharePoint Online via Microsoft Graph simple upload.
    /// </summary>
    public static class Program
    {
        #region Public Functions
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Zero on success; non-zero on failure.</returns>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                Console.WriteLine("Load Configuration");
                IConfiguration Config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                Console.WriteLine("Reading Settings");
                SharePointSettings SharePointSettings = new SharePointSettings();
                Config.GetSection("SharePoint").Bind(SharePointSettings);
                ValidateSettings(SharePointSettings);

                using SharePointUploaderService SharePointUploaderService = new SharePointUploaderService(SharePointSettings);

                Console.WriteLine("Retrieve SiteId");
                string SiteId = await SharePointUploaderService.GetSiteIdAsync(SharePointSettings.TenantHost, SharePointSettings.SitePath);
                Console.WriteLine($"SiteId : {SiteId}");

                Console.WriteLine("Retrieve DriveId");
                string DriveId = await SharePointUploaderService.GetDriveIdAsync(SiteId, SharePointSettings.LibraryName);
                Console.WriteLine($"DriveId : {DriveId}");

                // V1 uses simple PUT upload only. Larger/resumable uploads are demonstrated in UploadToSharepointV2.
                foreach (UploadFileSettings FileSettings in SharePointSettings.Files)
                {
                    string LocalPath = ResolveLocalPath(FileSettings.LocalPath);
                    Console.WriteLine($"Upload {Path.GetFileName(LocalPath)} → {FileSettings.TargetFolder}");

                    UploadResult Result = await SharePointUploaderService.UploadFileAsync(DriveId, LocalPath, FileSettings.TargetFolder);
                    Console.WriteLine($"  OK id={Result.DriveItemId} size={Result.Size} url={Result.WebUrl}");
                }

                Console.WriteLine("Done.");
                return 0;
            }
            catch (FileNotFoundException Exception)
            {
                Console.Error.WriteLine($"File error: {Exception.Message}");
                if (!string.IsNullOrWhiteSpace(Exception.FileName))
                    Console.Error.WriteLine($"  Path: {Exception.FileName}");
                return 1;
            }
            catch (HttpRequestException Exception)
            {
                Console.Error.WriteLine($"HTTP/Graph error: {Exception.Message}");
                return 2;
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLine($"Unexpected error: {Exception.Message}");
                return 3;
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Validates required settings and that all configured local files exist.
        /// </summary>
        /// <param name="settings">Bound SharePoint settings.</param>
        private static void ValidateSettings(SharePointSettings settings)
        {
            List<string> Errors = new List<string>();

            if (string.IsNullOrWhiteSpace(settings.TenantId))
                Errors.Add("SharePoint:TenantId is required.");
            if (string.IsNullOrWhiteSpace(settings.ClientId))
                Errors.Add("SharePoint:ClientId is required.");
            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
                Errors.Add("SharePoint:ClientSecret is required (use user secrets or environment variables).");
            if (string.IsNullOrWhiteSpace(settings.TenantHost))
                Errors.Add("SharePoint:TenantHost is required.");
            if (string.IsNullOrWhiteSpace(settings.SitePath))
                Errors.Add("SharePoint:SitePath is required.");
            if (string.IsNullOrWhiteSpace(settings.LibraryName))
                Errors.Add("SharePoint:LibraryName is required.");
            if (settings.Files is null || settings.Files.Count == 0)
                Errors.Add("SharePoint:Files must contain at least one file entry.");

            if (settings.Files is not null)
            {
                for (int Index = 0; Index < settings.Files.Count; Index++)
                {
                    UploadFileSettings FileSettings = settings.Files[Index];
                    if (string.IsNullOrWhiteSpace(FileSettings.LocalPath))
                    {
                        Errors.Add($"SharePoint:Files[{Index}].LocalPath is required.");
                        continue;
                    }

                    string ResolvedPath = ResolveLocalPath(FileSettings.LocalPath);
                    if (!File.Exists(ResolvedPath))
                        Errors.Add($"SharePoint:Files[{Index}] not found: {ResolvedPath}");
                }
            }

            if (Errors.Count > 0)
                throw new InvalidOperationException("Configuration validation failed:" + Environment.NewLine + string.Join(Environment.NewLine, Errors));
        }

        /// <summary>
        /// Resolves a local path against the application base directory when the path is not rooted.
        /// </summary>
        /// <param name="localPath">Configured local path.</param>
        /// <returns>An absolute file path.</returns>
        private static string ResolveLocalPath(string localPath)
        {
            if (Path.IsPathRooted(localPath))
                return Path.GetFullPath(localPath);

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, localPath));
        }
        #endregion
    }
}
