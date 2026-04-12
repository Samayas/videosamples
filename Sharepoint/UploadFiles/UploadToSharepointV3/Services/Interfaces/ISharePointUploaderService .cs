namespace UploadFiles.UploadToSharepointV3.Services
{
    public interface ISharePointUploaderService : IDisposable
    {
        Task<string> UploadFileAsync(string driveId, string localFilePath, string targetFolder = "");

        Task<string> GetSiteIdAsync(string tenantHost, string sitePath);

        Task<string> GetDriveIdAsync(string siteId, string libraryName = "Documents");

        Task PatchMetadataAsync(string driveId, string driveItemId, Dictionary<string, object> fields);
    }
}
