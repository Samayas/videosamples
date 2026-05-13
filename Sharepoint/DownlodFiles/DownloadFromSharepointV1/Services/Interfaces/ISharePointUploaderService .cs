using DownloadFiles.DownloadFromSharepointV1.Models;

namespace DownloadFiles.DownloadFromSharepointV1.Services
{
    public interface ISharePointUploaderService : IDisposable
    {
        Task<string> UploadFileAsync(string driveId, string localFilePath, string targetFolder = "");

        Task<string> UploadWithMetadataAsync(string driveId, string localFilePath, Dictionary<string, object> metadata, string targetFolder = "");

        Task<string> UploadWithMetadataAsync(string driveId, string fileName, Stream fileStream, Dictionary<string, object> metadata, string targetFolder = "");

        Task<string> GetSiteIdAsync(string tenantHost, string sitePath);

        Task<string> GetDriveIdAsync(string siteId, string libraryName = "Documents");

        Task PatchMetadataAsync(string driveId, string driveItemId, Dictionary<string, object> fields);

        Task<IReadOnlyList<DriveItemInfo>> BrowseDirectoryAsync(string driveId, string? folderPath = null);

        Task<IReadOnlyList<DriveItemInfo>> ListFilesRecursiveAsync(string driveId, string? folderPath = null);

        Task<DownloadedFile> DownloadFileWithMetadataAsync(string driveId, string driveItemId);

        Task<(string FilePath, IReadOnlyDictionary<string, object?> Metadata)> DownloadFileToDirectoryAsync(string driveId, string driveItemId, string destinationDirectory);
    }
}
