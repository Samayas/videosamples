namespace UploadFiles.UploadToSharepointV2.Services
{
    public interface ISharePointUploaderService : IDisposable
    {
        Task UploadFileAsync(string driveId, string localFilePath, string targetFolder = "");

        Task<string> GetSiteIdAsync(string tenantHost, string sitePath);

        Task<string> GetDriveIdAsync(string siteId, string libraryName = "Documents");
    }
}
