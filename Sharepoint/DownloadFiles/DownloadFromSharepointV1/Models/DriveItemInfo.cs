namespace DownloadFiles.DownloadFromSharepointV1.Models
{
    public sealed class DriveItemInfo
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public bool IsFolder { get; init; }
        /// <summary>Null for folders.</summary>
        public long? SizeBytes { get; init; }
        public DateTimeOffset? CreatedDateTime { get; init; }
        public DateTimeOffset? LastModifiedDateTime { get; init; }
        public string? DownloadUrl { get; init; }
    }
}
