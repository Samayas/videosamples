namespace UploadFiles.UploadToSharepointV1.Settings
{
    /// <summary>
    /// Describes a single local file to upload into SharePoint.
    /// </summary>
    public sealed class UploadFileSettings
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the local file path.
        /// Relative paths are resolved against the application base directory.
        /// </summary>
        public string LocalPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional folder under the library root, for example <c>App1</c>.
        /// </summary>
        public string TargetFolder { get; set; } = string.Empty;
        #endregion
    }
}
