using Microsoft.AspNetCore.Mvc;
using Wangkanai.Detection.Services;
using Wangkanai.Detection.Models;

namespace ImageHandling.Controllers
{
    [Route("responsive-imageadv")]
    public class ImageAdvancedController : Controller
    {
        private readonly IDetectionService detectionService;
        private readonly IWebHostEnvironment environment;

        public ImageAdvancedController(IWebHostEnvironment environment, IDetectionService detectionService)
        {
            this.detectionService = detectionService;
            this.environment = environment;
        }

        [HttpGet("{imageName}")]
        public IActionResult GetResponsiveImage(string imageName)
        {
            string deviceType = this.detectionService.Device.Type switch
            {
                Device.Mobile => "mobile",
                Device.Tablet => "tablet",
                _ => "desktop"
            };

            // Build path to the correct image
            string imagePath = Path.Combine($"wwwroot/images/{deviceType}/{imageName}");

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            string contentType = GetContentType(imagePath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(imagePath);

            return File(fileBytes, contentType);
        }

        private string GetContentType(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
