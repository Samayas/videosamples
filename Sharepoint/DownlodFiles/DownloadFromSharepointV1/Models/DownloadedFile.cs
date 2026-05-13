namespace DownloadFiles.DownloadFromSharepointV1.Models
{
    public sealed class DownloadedFile : IDisposable
    {
        public string FileName { get; init; } = string.Empty;
        public Stream Content { get; init; } = Stream.Null;
        /// <summary>SharePoint column values (listItem/fields).</summary>
        public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
        public void Dispose() 
        { 
            Content.Dispose(); 
        }
    }
}
