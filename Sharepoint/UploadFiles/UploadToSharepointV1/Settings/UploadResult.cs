namespace UploadFiles.UploadToSharepointV1.Settings
{
    /// <summary>
    /// Result details returned after a successful simple Graph upload.
    /// </summary>
    public sealed class UploadResult
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the drive item identifier.
        /// </summary>
        public string DriveItemId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the uploaded file name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the uploaded file size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the web URL of the uploaded item when available.
        /// </summary>
        public string WebUrl { get; set; } = string.Empty;
        #endregion
    }
}
