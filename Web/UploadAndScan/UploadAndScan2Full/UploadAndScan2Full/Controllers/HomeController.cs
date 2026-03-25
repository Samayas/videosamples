using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using  UploadAndScan2Full.Models;
using  UploadAndScan2Full.Services;

namespace UploadAndScan2Full.Controllers
{
    public class HomeController : Controller
    {
        private readonly string uploadPath;
        private readonly IClamAvScannerService clamAvScanner;

        public HomeController(IConfiguration configuration, IClamAvScannerService clamAvScanner)
        {
            this.uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            this.clamAvScanner = clamAvScanner;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
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

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            List<string> uploadedFiles = new List<string>();

            foreach (IFormFile file in model.Files)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string uniqueFileNme = Guid.NewGuid() + "." + fileName;
                        string filePath = Path.Combine(uploadPath, uniqueFileNme);

                        using (Stream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        ScanResult scanResult = await clamAvScanner.ScanFileAsync(filePath);

                        if (scanResult.IsInfected)
                        {
                            System.IO.File.Delete(filePath);

                            ModelState.AddModelError("", $"File {fileName} is infected: {scanResult.Message}");
                            return View(model);
                        }

                        uploadedFiles.Add(fileName);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error uploading file {file.FileName}: {ex.Message}");

                        return View(model);
                    }
                }
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
