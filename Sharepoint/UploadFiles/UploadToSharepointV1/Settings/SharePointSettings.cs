namespace UploadFiles.UploadToSharepointV1.Settings
{
    /// <summary>
    /// Configuration for Microsoft Graph / SharePoint app-only authentication and upload targets.
    /// </summary>
    public sealed class SharePointSettings
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the Entra ID tenant identifier.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the application (client) identifier.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the application client secret.
        /// Prefer user secrets or environment variables over committing real values.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SharePoint host name, for example <c>contoso.sharepoint.com</c>.
        /// </summary>
        public string TenantHost { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the site-relative path, for example <c>/sites/UploadFiles</c>.
        /// </summary>
        public string SitePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document library display name.
        /// </summary>
        public string LibraryName { get; set; } = "Documents";

        /// <summary>
        /// Gets or sets the files to upload.
        /// </summary>
        public List<UploadFileSettings> Files { get; set; } = new List<UploadFileSettings>();
        #endregion
    }
}
