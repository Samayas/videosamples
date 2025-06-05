using Microsoft.AspNetCore.Mvc;

namespace ImageHandling.Controllers
{
    [Route("responsive-imageV3")]
    public class ImageControllerV3 : Controller
    {
        [HttpGet("{imageName}")]
        public IActionResult GetResponsiveImage(string imageName, [FromQuery] string? size)
        {
            string deviceType = size;

            if (string.IsNullOrEmpty(deviceType))
            {
                string userAgent = Request.Headers["User-Agent"].ToString().ToLower();

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
