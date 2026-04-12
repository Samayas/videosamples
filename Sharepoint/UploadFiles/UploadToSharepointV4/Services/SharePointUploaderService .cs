using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UploadFiles.UploadToSharepointV4.Settings;

namespace UploadFiles.UploadToSharepointV4.Services
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

        public async Task UploadWithMetadataAsync(string driveId, string localFilePath, Dictionary<string, object> metadata, string targetFolder = "")
        {
            string[] requiredFields = { "User", "Confidential" };
            foreach (string field in requiredFields)
            {
                if (!metadata.ContainsKey(field) || metadata[field] is null || string.IsNullOrWhiteSpace(metadata[field].ToString()))
                    throw new ArgumentException($"Required metadata field '{field}' is missing or empty.");
            }

            string driveItemId = await UploadFileAsync(driveId, localFilePath, targetFolder);
            await PatchMetadataAsync(driveId, driveItemId, metadata);
        }

        public async Task UploadWithMetadataAsync(string driveId, string fileName, Stream fileStream, Dictionary<string, object> metadata, string targetFolder = "")
        {
            string[] requiredFields = { "User", "Confidential" };
            foreach (string field in requiredFields)
            {
                if (!metadata.ContainsKey(field) || metadata[field] is null || string.IsNullOrWhiteSpace(metadata[field].ToString()))
                    throw new ArgumentException($"Required metadata field '{field}' is missing or empty.");
            }

            string driveItemId = await UploadFileAsync(driveId, fileName, fileStream, targetFolder);
            await PatchMetadataAsync(driveId, driveItemId, metadata);
        }

        public async Task<string> UploadFileAsync(string driveId, string localFilePath, string targetFolder = "")
        {
            string token = await GetAccessTokenAsync();
            string fileName = Path.GetFileName(localFilePath);
            string remotePath = string.IsNullOrWhiteSpace(targetFolder) ? fileName : $"{targetFolder.Trim('/')}/{fileName}";

            using FileStream fileStream = File.OpenRead(localFilePath);
            long fileSize = fileStream.Length;

            if (fileSize <= SmallFileThreshold)
            {
                return await UploadSmallAsync(driveId, remotePath, fileStream, token);
            }
            else
            {
                return await UploadLargeAsync(driveId, remotePath, fileStream, fileSize, token);
            }
        }

        private async Task<string> UploadFileAsync(string driveId, string fileName, Stream fileStream, string targetFolder = "")
        {
            string token = await GetAccessTokenAsync();
            string remotePath = string.IsNullOrWhiteSpace(targetFolder) ? fileName : $"{targetFolder.Trim('/')}/{fileName}";
            long fileSize = fileStream.Length;

            if (fileSize <= SmallFileThreshold)
            {
                return await UploadSmallAsync(driveId, remotePath, fileStream, token);
            }
            else
            {
                return await UploadLargeAsync(driveId, remotePath, fileStream, fileSize, token);
            }
        }

        public async Task PatchMetadataAsync(string driveId, string driveItemId, Dictionary<string, object> fields)
        {
            string token = await GetAccessTokenAsync();
            string url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{driveItemId}/listItem/fields";
            string body = JsonSerializer.Serialize(fields);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Metadata patch failed ({response.StatusCode}): {error}");
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private async Task<string> UploadSmallAsync(string driveId, string remotePath, Stream fileStream, string token)
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

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("id").GetString() ?? throw new InvalidOperationException("DriveItem ID missing from upload response.");
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

        private async Task<string> UploadLargeAsync(string driveId, string remotePath, Stream fileStream, long fileSize, string token)
        {
            string uploadUrl = await CreateUploadSessionAsync(driveId, remotePath, token);

            byte[] buffer = new byte[ChunkSize];
            long offset = 0;
            int chunk = 0;
            string driveItemId = string.Empty;

            while (offset < fileSize)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, 0, ChunkSize);
                long rangeEnd = offset + bytesRead - 1;
                chunk++;

                using HttpRequestMessage chunkRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                chunkRequest.Content = new ByteArrayContent(buffer, 0, bytesRead);
                chunkRequest.Content.Headers.Add("Content-Range", $"bytes {offset}-{rangeEnd}/{fileSize}");
                chunkRequest.Content.Headers.ContentLength = bytesRead;

                HttpResponseMessage chunkResponse = await httpClient.SendAsync(chunkRequest);
                if (chunkResponse.StatusCode == System.Net.HttpStatusCode.OK || chunkResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    string body = await chunkResponse.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(body);
                    driveItemId = doc.RootElement.GetProperty("id").GetString() ?? throw new InvalidOperationException("DriveItem ID missing from final chunk response.");
                }
                else if (chunkResponse.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    string error = await chunkResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Chunk {chunk} failed ({chunkResponse.StatusCode}): {error}");
                }

                offset += bytesRead;
            }

            if (string.IsNullOrEmpty(driveItemId))
                throw new InvalidOperationException("Upload completed but DriveItem ID was never returned.");

            return driveItemId;
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
