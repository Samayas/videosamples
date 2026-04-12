using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using UploadAndScan1Upload.Models;
using UploadFiles.UploadToSharepointV4.Services;
using UploadFiles.UploadToSharepointV4.Settings;

namespace UploadFiles.UploadToSharepointV3
{
    public class HomeController : Controller
    {
        private readonly SharePointSettings sharePointSettings;

        public HomeController(IOptions<SharePointSettings> sharePointSettings)
        {
            this.sharePointSettings = sharePointSettings.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
//        [RequestSizeLimit(8_388_608)] 
        public async Task<IActionResult> Index(UploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Files == null || model.Files.Count == 0)
            {
                ModelState.AddModelError("", "No files selected.");

                return View(model);
            }

            SharePointUploaderService sharePointUploaderService = new SharePointUploaderService(sharePointSettings);

            string siteId = await sharePointUploaderService.GetSiteIdAsync("samayas.sharepoint.com", "/sites/UploadFiles");
            string driveId = await sharePointUploaderService.GetDriveIdAsync(siteId, "Documents");

            List<string> uploadedFiles = new List<string>();
            List<string> failedFiles = new List<string>();

            foreach (IFormFile file in model.Files)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file.FileName);

                        Dictionary<string, object> metadata = new Dictionary<string, object>
                        {
                            { "User", model.UploadedBy },
                            { "Confidential", model.Confidentiality }
                        };

                        using Stream fileStream = file.OpenReadStream();

                        await sharePointUploaderService.UploadWithMetadataAsync(driveId, fileName, fileStream, metadata, "App1");

                        uploadedFiles.Add(fileName);
                    }
                    catch (Exception ex)
                    {
                        failedFiles.Add($"{file.FileName}: {ex.Message}");

                        return View(model);
                    }
                }
            }

            if (failedFiles.Any())
            {
                ModelState.AddModelError("", $"Failed uploads: {string.Join(", ", failedFiles)}");
            }

            if (!uploadedFiles.Any())
            {
                ModelState.AddModelError("", "No files were uploaded.");

                return View(model);
            }

            if (uploadedFiles.Count > 0)
            {
                TempData["SuccessMessage"] = $"{uploadedFiles.Count} file(s) uploaded successfully: " + string.Join(", ", uploadedFiles);
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
