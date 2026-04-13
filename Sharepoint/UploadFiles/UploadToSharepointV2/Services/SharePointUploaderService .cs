using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UploadFiles.UploadToSharepointV2.Settings;

namespace UploadFiles.UploadToSharepointV2.Services
{
    public class SharePointUploaderService : ISharePointUploaderService
    {
        private readonly SharePointSettings sharePointSettings;
        private readonly HttpClient httpClient;

        // 4 MB
        private const long SmallFileThreshold = 4 * 1024 * 1024;
        // 1.6 MB — must be a multiple of 320 KiB
        private const int ChunkSize = 5 * 320 * 1024;

        public SharePointUploaderService(SharePointSettings sharePointSettings)
        {
            this.sharePointSettings = sharePointSettings;
            this.httpClient = new HttpClient();
        }

        public async Task<string> GetSiteIdAsync(string tenantHost, string sitePath)
        {
            string token = await GetAccessTokenAsync();
            string url = $"https://graph.microsoft.com/v1.0/sites/{tenantHost}:{sitePath}?$select=id";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GetSiteId failed ({response.StatusCode}): {error}");
            }

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            return doc.RootElement.GetProperty("id").GetString() ?? throw new InvalidOperationException("Site ID not found in response.");
        }

        public async Task<string> GetDriveIdAsync(string siteId, string libraryName = "Documents")
        {
            string token = await GetAccessTokenAsync();
            string url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives?$select=id,name";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GetDriveId failed ({response.StatusCode}): {error}");
            }

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement drives = doc.RootElement.GetProperty("value");

            foreach (JsonElement drive in drives.EnumerateArray())
            {
                string name = drive.GetProperty("name").GetString() ?? string.Empty;
                if (string.Equals(name, libraryName, StringComparison.OrdinalIgnoreCase))
                    return drive.GetProperty("id").GetString() ?? throw new InvalidOperationException("Drive ID was null.");
            }

            throw new InvalidOperationException($"No document library named '{libraryName}' found on site '{siteId}'.");
        }

        public async Task UploadFileAsync(string driveId, string localFilePath, string targetFolder = "")
        {
            string token = await GetAccessTokenAsync();
            string fileName = Path.GetFileName(localFilePath);
            string remotePath = string.IsNullOrWhiteSpace(targetFolder) ? fileName : $"{targetFolder.Trim('/')}/{fileName}";

            using FileStream fileStream = File.OpenRead(localFilePath);
            long fileSize = fileStream.Length;

            if (fileSize <= SmallFileThreshold)
            {
                await UploadSmallAsync(driveId, remotePath, fileStream, token);
            }
            else
            {
                await UploadLargeAsync(driveId, remotePath, fileStream, fileSize, token);
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private async Task UploadSmallAsync(string driveId, string remotePath, Stream fileStream, string token)
        {
            string url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{remotePath}:/content";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StreamContent(fileStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Upload failed ({response.StatusCode}): {error}");
            }
        }

        private async Task<string> CreateUploadSessionAsync(string driveId, string remotePath, string token)
        {
            string url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{remotePath}:/createUploadSession";
            string body = """{"item": {"@microsoft.graph.conflictBehavior": "replace"}}""";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"CreateUploadSession failed ({response.StatusCode}): {error}");
            }

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            return doc.RootElement.GetProperty("uploadUrl").GetString() ?? throw new InvalidOperationException("uploadUrl missing from session response.");
        }

        private async Task UploadLargeAsync(string driveId, string remotePath, Stream fileStream, long fileSize, string token)
        {
            string uploadUrl = await CreateUploadSessionAsync(driveId, remotePath, token);

            byte[] buffer = new byte[ChunkSize];
            long offset = 0;
            int chunk = 0;

            while (offset < fileSize)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, 0, ChunkSize);
                long rangeEnd = offset + bytesRead - 1;
                chunk++;

                Console.WriteLine($"  Uploading chunk {chunk}: bytes {offset}-{rangeEnd} of {fileSize}");

                using HttpRequestMessage chunkRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                chunkRequest.Content = new ByteArrayContent(buffer, 0, bytesRead);
                chunkRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                chunkRequest.Content.Headers.Add("Content-Range", $"bytes {offset}-{rangeEnd}/{fileSize}");
                chunkRequest.Content.Headers.ContentLength = bytesRead;

                HttpResponseMessage chunkResponse = await httpClient.SendAsync(chunkRequest);
                if (chunkResponse.StatusCode != System.Net.HttpStatusCode.Accepted && chunkResponse.StatusCode != System.Net.HttpStatusCode.OK && chunkResponse.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    string error = await chunkResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Chunk {chunk} failed ({chunkResponse.StatusCode}): {error}");
                }

                offset += bytesRead;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            string tokenUrl = $"https://login.microsoftonline.com/{sharePointSettings.TenantId}/oauth2/v2.0/token";

            Dictionary<string, string> form = new Dictionary<string, string>
            {
                { "client_id", sharePointSettings.ClientId },
                { "client_secret", sharePointSettings.ClientSecret },
                { "scope", "https://graph.microsoft.com/.default" },
                { "grant_type", "client_credentials" }
            };

            HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(form));
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Token request failed ({response.StatusCode}): {error}");
            }

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            return doc.RootElement.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("access_token missing from token response.");
        }
    }
}
