using Microsoft.AspNetCore.Mvc;

namespace ImageHandling.Controllers
{
    [Route("responsive-imageV4")]
    public class ImageControllerV4 : Controller
    {
        [HttpGet("{imageName}")]
        public IActionResult GetResponsiveImage(string imageName)
        {
            string deviceType = Request.Cookies["deviceType"];

            if (string.IsNullOrEmpty(deviceType))
            {
                string userAgent = Request.Headers["User-Agent"].ToString().ToLower();

                // Example logic (can be adjusted)
                if (userAgent.Contains("mobile"))
                {
                    deviceType = "mobile";
                }
                else if (userAgent.Contains("tablet") || userAgent.Contains("android"))
                {
                    deviceType = "tablet";
                }
                else
                {
                    deviceType = "desktop";
                }
            }

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
