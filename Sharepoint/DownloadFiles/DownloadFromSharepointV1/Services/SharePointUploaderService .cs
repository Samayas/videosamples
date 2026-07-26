using DownloadFiles.DownloadFromSharepointV1.Models;
using DownloadFiles.DownloadFromSharepointV1.Settings;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DownloadFiles.DownloadFromSharepointV1.Services
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

        public async Task<string> UploadWithMetadataAsync(string driveId, string localFilePath, Dictionary<string, object> metadata, string targetFolder = "")
        {
            ValidateRequiredMetadata(metadata);
            string driveItemId = await UploadFileAsync(driveId, localFilePath, targetFolder);
            await PatchMetadataAsync(driveId, driveItemId, metadata);
            return driveItemId;
        }

        public async Task<string> UploadWithMetadataAsync(string driveId, string fileName, Stream fileStream, Dictionary<string, object> metadata, string targetFolder = "")
        {
            ValidateRequiredMetadata(metadata);
            string driveItemId = await UploadFileInternalAsync(driveId, fileName, fileStream, targetFolder);
            await PatchMetadataAsync(driveId, driveItemId, metadata);
            return driveItemId;
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

        public async Task<IReadOnlyList<DriveItemInfo>> BrowseDirectoryAsync(string driveId, string? folderPath = null)
        {
            string token = await GetAccessTokenAsync();
            // Root children vs. specific-folder children
            string url = string.IsNullOrWhiteSpace(folderPath)
                ? $"https://graph.microsoft.com/v1.0/drives/{driveId}/root/children"
                  + "?$select=id,name,folder,size,createdDateTime,lastModifiedDateTime,@microsoft.graph.downloadUrl"
                : $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{Uri.EscapeDataString(folderPath.Trim('/'))}:/children"
                  + "?$select=id,name,folder,size,createdDateTime,lastModifiedDateTime,@microsoft.graph.downloadUrl";

            return await GetAllPagesAsync(url, token, ParseDriveItemInfo);
        }

        public async Task<IReadOnlyList<DriveItemInfo>> ListFilesRecursiveAsync(
           string driveId, string? folderPath = null)
        {
            List<DriveItemInfo> result = new List<DriveItemInfo>();
            await CollectFilesAsync(driveId, folderPath, result, await GetAccessTokenAsync());
            return result.AsReadOnly();
        }

        public async Task<DownloadedFile> DownloadFileWithMetadataAsync(string driveId, string driveItemId)
        {
            string token = await GetAccessTokenAsync();

            // ── 1. Get item info (name + pre-auth download URL) ──────────────
            string itemUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{driveItemId}" + "?$select=name,@microsoft.graph.downloadUrl";

            using HttpRequestMessage itemRequest = new HttpRequestMessage(HttpMethod.Get, itemUrl);
            itemRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage itemResponse = await httpClient.SendAsync(itemRequest);
            if (!itemResponse.IsSuccessStatusCode)
            {
                string error = await itemResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GetItem failed ({itemResponse.StatusCode}): {error}");
            }

            string itemJson = await itemResponse.Content.ReadAsStringAsync();
            using JsonDocument itemDoc = JsonDocument.Parse(itemJson);

            string fileName = itemDoc.RootElement.GetProperty("name").GetString() ?? driveItemId;
            string downloadUrl = itemDoc.RootElement
                                        .GetProperty("@microsoft.graph.downloadUrl")
                                        .GetString()
                                 ?? throw new InvalidOperationException("Download URL missing from item response.");

            // ── 2. Stream the file content ────────────────────────────────────
            // The pre-auth URL does NOT require an Authorization header.
            HttpResponseMessage contentResponse = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

            if (!contentResponse.IsSuccessStatusCode)
            {
                string error = await contentResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Download failed ({contentResponse.StatusCode}): {error}");
            }

            MemoryStream buffer = new MemoryStream();
            await contentResponse.Content.CopyToAsync(buffer);
            buffer.Position = 0;

            // ── 3. Fetch SharePoint column metadata (listItem/fields) ─────────
            IReadOnlyDictionary<string, object?> metadata = await GetItemMetadataAsync(driveId, driveItemId, token);

            return new DownloadedFile
            {
                FileName = fileName,
                Content = buffer,
                Metadata = metadata,
            };
        }

        public async Task<(string FilePath, IReadOnlyDictionary<string, object?> Metadata)> DownloadFileToDirectoryAsync(string driveId, string driveItemId, string destinationDirectory)
        {
            using DownloadedFile downloaded = await DownloadFileWithMetadataAsync(driveId, driveItemId);

            Directory.CreateDirectory(destinationDirectory);
            string filePath = Path.Combine(destinationDirectory, downloaded.FileName);

            await using FileStream fs = File.Create(filePath);
            await downloaded.Content.CopyToAsync(fs);

            return (filePath, downloaded.Metadata);
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

                Console.WriteLine($"  Uploading chunk {chunk}: bytes {offset}-{rangeEnd} of {fileSize}");

                using HttpRequestMessage chunkRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                chunkRequest.Content = new ByteArrayContent(buffer, 0, bytesRead);
                chunkRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
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

        private async Task<IReadOnlyDictionary<string, object?>> GetItemMetadataAsync(string driveId, string driveItemId, string token)
        {
            string url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{driveItemId}/listItem/fields";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GetMetadata failed ({response.StatusCode}): {error}");
            }

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            Dictionary<string, object?> fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                fields[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.TryGetInt64(out long l) ? l : prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => prop.Value.GetRawText(),
                };
            }

            return fields;
        }

        private async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(string url, string token, Func<JsonElement, T> mapper)
        {
            List<T> items = new List<T>();
            string? nextLink = url;

            while (nextLink is not null)
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, nextLink);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Graph paged request failed ({response.StatusCode}): {error}");
                }

                using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                JsonElement root = doc.RootElement;

                foreach (JsonElement element in root.GetProperty("value").EnumerateArray())
                    items.Add(mapper(element));

                nextLink = root.TryGetProperty("@odata.nextLink", out JsonElement next)
                    ? next.GetString()
                    : null;
            }

            return items.AsReadOnly();
        }

        private async Task CollectFilesAsync(string driveId, string? folderPath, List<DriveItemInfo> result, string token)
        {
            IReadOnlyList<DriveItemInfo> children = await BrowseDirectoryAsync(driveId, folderPath);

            foreach (DriveItemInfo item in children)
            {
                if (item.IsFolder)
                {
                    string childPath = string.IsNullOrWhiteSpace(folderPath)
                        ? item.Name
                        : $"{folderPath.TrimEnd('/')}/{item.Name}";
                    await CollectFilesAsync(driveId, childPath, result, token);
                }
                else
                {
                    result.Add(item);
                }
            }
        }

        private static DriveItemInfo ParseDriveItemInfo(JsonElement element)
        {
            bool isFolder = element.TryGetProperty("folder", out _);

            DateTimeOffset? created = element.TryGetProperty("createdDateTime", out JsonElement c) && DateTimeOffset.TryParse(c.GetString(), out DateTimeOffset cdt) ? cdt : null;

            DateTimeOffset? modified = element.TryGetProperty("lastModifiedDateTime", out JsonElement m) && DateTimeOffset.TryParse(m.GetString(), out DateTimeOffset mdt) ? mdt : null;

            long? size = !isFolder && element.TryGetProperty("size", out JsonElement s) ? s.GetInt64() : null;

            string? downloadUrl = !isFolder && element.TryGetProperty("@microsoft.graph.downloadUrl", out JsonElement dl) ? dl.GetString() : null;

            return new DriveItemInfo
            {
                Id = element.GetProperty("id").GetString() ?? string.Empty,
                Name = element.GetProperty("name").GetString() ?? string.Empty,
                IsFolder = isFolder,
                SizeBytes = size,
                CreatedDateTime = created,
                LastModifiedDateTime = modified,
                DownloadUrl = downloadUrl,
            };
        }

        private async Task<string> UploadFileInternalAsync(string driveId, string fileName, Stream fileStream, string targetFolder = "")
        {
            string token = await GetAccessTokenAsync();
            string remotePath = BuildRemotePath(fileName, targetFolder);
            long fileSize = fileStream.Length;

            return fileSize <= SmallFileThreshold
                ? await UploadSmallAsync(driveId, remotePath, fileStream, token)
                : await UploadLargeAsync(driveId, remotePath, fileStream, fileSize, token);
        }

        private static string BuildRemotePath(string fileName, string targetFolder) => string.IsNullOrWhiteSpace(targetFolder)
                ? fileName
                : $"{targetFolder.Trim('/')}/{fileName}";

        private static void ValidateRequiredMetadata(Dictionary<string, object> metadata)
        {
            string[] requiredFields = { "User", "Confidential" };
            foreach (string field in requiredFields)
            {
                if (!metadata.ContainsKey(field)
                    || metadata[field] is null
                    || string.IsNullOrWhiteSpace(metadata[field].ToString()))
                    throw new ArgumentException($"Required metadata field '{field}' is missing or empty.");
            }
        }
    }
}
