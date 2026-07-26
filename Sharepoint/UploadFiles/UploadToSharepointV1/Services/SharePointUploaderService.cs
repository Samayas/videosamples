#region Usings
using System.Net.Http.Headers;
using System.Text.Json;

using UploadFiles.UploadToSharepointV1.Settings;
#endregion

namespace UploadFiles.UploadToSharepointV1.Services
{
    /// <summary>
    /// Microsoft Graph client that authenticates with client credentials and performs simple file uploads.
    /// V1 intentionally uses only PUT .../content. Prefer UploadToSharepointV2 for files above ~10 MiB.
    /// </summary>
    public sealed class SharePointUploaderService : ISharePointUploaderService
    {
        #region Declarations
        // Constants
        private const int TokenRefreshSkewSeconds = 60;

        // Private variables
        private readonly SharePointSettings SharePointSettings;
        private readonly HttpClient HttpClient;
        private string CachedAccessToken = string.Empty;
        private DateTimeOffset AccessTokenExpiresAt = DateTimeOffset.MinValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointUploaderService"/> class.
        /// </summary>
        /// <param name="sharePointSettings">SharePoint and Entra ID settings.</param>
        public SharePointUploaderService(SharePointSettings sharePointSettings)
        {
            this.SharePointSettings = sharePointSettings ?? throw new ArgumentNullException(nameof(sharePointSettings));
            this.HttpClient = new HttpClient();
        }
        #endregion

        #region Public Functions
        /// <inheritdoc />
        public async Task<string> GetSiteIdAsync(string tenantHost, string sitePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantHost))
                throw new ArgumentException("Tenant host is required.", nameof(tenantHost));
            if (string.IsNullOrWhiteSpace(sitePath))
                throw new ArgumentException("Site path is required.", nameof(sitePath));

            string Token = await this.GetAccessTokenAsync(cancellationToken);
            string Url = $"https://graph.microsoft.com/v1.0/sites/{tenantHost}:{sitePath}?$select=id";

            using JsonDocument Document = await this.SendGraphAsync(HttpMethod.Get, Url, Token, content: null, "GetSiteId", cancellationToken);
            return Document.RootElement.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Site ID not found in response.");
        }

        /// <inheritdoc />
        public async Task<string> GetDriveIdAsync(string siteId, string libraryName = "Documents", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new ArgumentException("Site ID is required.", nameof(siteId));
            if (string.IsNullOrWhiteSpace(libraryName))
                throw new ArgumentException("Library name is required.", nameof(libraryName));

            string Token = await this.GetAccessTokenAsync(cancellationToken);
            string Url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives?$select=id,name";

            using JsonDocument Document = await this.SendGraphAsync(HttpMethod.Get, Url, Token, content: null, "GetDriveId", cancellationToken);
            JsonElement Drives = Document.RootElement.GetProperty("value");

            foreach (JsonElement Drive in Drives.EnumerateArray())
            {
                string Name = Drive.GetProperty("name").GetString() ?? string.Empty;
                if (string.Equals(Name, libraryName, StringComparison.OrdinalIgnoreCase))
                {
                    return Drive.GetProperty("id").GetString()
                        ?? throw new InvalidOperationException("Drive ID was null.");
                }
            }

            throw new InvalidOperationException($"No document library named '{libraryName}' found on site '{siteId}'.");
        }

        /// <inheritdoc />
        public async Task<UploadResult> UploadFileAsync(string driveId, string localFilePath, string targetFolder = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(driveId))
                throw new ArgumentException("Drive ID is required.", nameof(driveId));
            if (string.IsNullOrWhiteSpace(localFilePath))
                throw new ArgumentException("Local file path is required.", nameof(localFilePath));
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("Local file was not found.", localFilePath);

            string Token = await this.GetAccessTokenAsync(cancellationToken);
            string FileName = Path.GetFileName(localFilePath);
            string RemotePath = this.BuildRemotePath(targetFolder, FileName);
            string EncodedRemotePath = this.EncodePathSegments(RemotePath);
            string Url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{EncodedRemotePath}:/content";

            // Simple upload replaces an existing item with the same path.
            using FileStream FileStream = File.OpenRead(localFilePath);
            using StreamContent Content = new StreamContent(FileStream);
            Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using JsonDocument Document = await this.SendGraphAsync(HttpMethod.Put, Url, Token, Content, "Upload", cancellationToken);
            JsonElement Root = Document.RootElement;

            return new UploadResult
            {
                DriveItemId = Root.GetProperty("id").GetString() ?? throw new InvalidOperationException("DriveItem ID missing from upload response."),
                Name = Root.TryGetProperty("name", out JsonElement NameElement) ? NameElement.GetString() ?? FileName : FileName,
                Size = Root.TryGetProperty("size", out JsonElement SizeElement) ? SizeElement.GetInt64() : FileStream.Length,
                WebUrl = Root.TryGetProperty("webUrl", out JsonElement WebUrlElement) ? WebUrlElement.GetString() ?? string.Empty : string.Empty
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.HttpClient.Dispose();
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Builds the library-relative remote path for a file.
        /// </summary>
        /// <param name="targetFolder">Optional folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <returns>Remote path without a leading slash.</returns>
        private string BuildRemotePath(string targetFolder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(targetFolder))
                return fileName;

            return $"{targetFolder.Trim('/')}/{fileName}";
        }

        /// <summary>
        /// URL-encodes each path segment while preserving separators.
        /// </summary>
        /// <param name="remotePath">Remote path with forward-slash separators.</param>
        /// <returns>Encoded path suitable for Graph item path URLs.</returns>
        private string EncodePathSegments(string remotePath)
        {
            string[] Segments = remotePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int Index = 0; Index < Segments.Length; Index++)
                Segments[Index] = Uri.EscapeDataString(Segments[Index]);

            return string.Join('/', Segments);
        }

        /// <summary>
        /// Returns a cached access token or requests a new one with client credentials.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Bearer access token.</returns>
        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.CachedAccessToken) && DateTimeOffset.UtcNow < this.AccessTokenExpiresAt)
                return this.CachedAccessToken;

            string TokenUrl = $"https://login.microsoftonline.com/{this.SharePointSettings.TenantId}/oauth2/v2.0/token";
            Dictionary<string, string> Form = new Dictionary<string, string>
            {
                { "client_id", this.SharePointSettings.ClientId },
                { "client_secret", this.SharePointSettings.ClientSecret },
                { "scope", "https://graph.microsoft.com/.default" },
                { "grant_type", "client_credentials" }
            };

            using FormUrlEncodedContent Content = new FormUrlEncodedContent(Form);
            HttpResponseMessage Response = await this.HttpClient.PostAsync(TokenUrl, Content, cancellationToken);
            string Body = await Response.Content.ReadAsStringAsync(cancellationToken);
            if (!Response.IsSuccessStatusCode)
                throw new HttpRequestException($"Token request failed ({Response.StatusCode}): {Body}");

            using JsonDocument Document = JsonDocument.Parse(Body);
            string AccessToken = Document.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("access_token missing from token response.");

            int ExpiresInSeconds = Document.RootElement.TryGetProperty("expires_in", out JsonElement ExpiresElement)
                ? ExpiresElement.GetInt32()
                : 3600;

            this.CachedAccessToken = AccessToken;
            this.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, ExpiresInSeconds - TokenRefreshSkewSeconds));
            return this.CachedAccessToken;
        }

        /// <summary>
        /// Sends an authorized Graph request and returns the JSON response body.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="url">Absolute Graph URL.</param>
        /// <param name="token">Bearer token.</param>
        /// <param name="content">Optional request content.</param>
        /// <param name="operationName">Name used in error messages.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Parsed JSON document. Caller must dispose it.</returns>
        private async Task<JsonDocument> SendGraphAsync(HttpMethod method, string url, string token, HttpContent? content, string operationName, CancellationToken cancellationToken)
        {
            using HttpRequestMessage Request = new HttpRequestMessage(method, url);
            Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (content is not null)
                Request.Content = content;

            HttpResponseMessage Response = await this.HttpClient.SendAsync(Request, cancellationToken);
            string Body = await Response.Content.ReadAsStringAsync(cancellationToken);
            if (!Response.IsSuccessStatusCode)
                throw new HttpRequestException($"{operationName} failed ({Response.StatusCode}): {Body}");

            return JsonDocument.Parse(Body);
        }
        #endregion
    }
}
