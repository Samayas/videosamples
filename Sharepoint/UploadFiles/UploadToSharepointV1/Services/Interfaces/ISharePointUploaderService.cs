#region Usings
using UploadFiles.UploadToSharepointV1.Settings;
#endregion

namespace UploadFiles.UploadToSharepointV1.Services
{
    /// <summary>
    /// Uploads files to a SharePoint document library using Microsoft Graph simple upload.
    /// </summary>
    public interface ISharePointUploaderService : IDisposable
    {
        /// <summary>
        /// Resolves the Microsoft Graph site identifier for the given host and site path.
        /// </summary>
        /// <param name="tenantHost">SharePoint host name, for example contoso.sharepoint.com.</param>
        /// <param name="sitePath">Site-relative path, for example /sites/UploadFiles.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The Graph site identifier.</returns>
        Task<string> GetSiteIdAsync(string tenantHost, string sitePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves the drive identifier for a document library on the given site.
        /// </summary>
        /// <param name="siteId">The Graph site identifier.</param>
        /// <param name="libraryName">Document library display name.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The drive identifier.</returns>
        Task<string> GetDriveIdAsync(string siteId, string libraryName = "Documents", CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a local file with a single PUT request (simple upload, intended for smaller files).
        /// For larger/resumable uploads, see UploadToSharepointV2.
        /// </summary>
        /// <param name="driveId">Target drive identifier.</param>
        /// <param name="localFilePath">Absolute or relative local file path.</param>
        /// <param name="targetFolder">Optional folder under the library root.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Details of the created or replaced drive item.</returns>
        Task<UploadResult> UploadFileAsync(string driveId, string localFilePath, string targetFolder = "", CancellationToken cancellationToken = default);
    }
}
