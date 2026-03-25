using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UploadAndScan1Upload.Models;

namespace UploadAndScan1Upload1Upload.Controllers
{
    public class HomeController : Controller
    {
        private readonly string uploadPath;

        public HomeController(IConfiguration configuration)
        {
            this.uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
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
