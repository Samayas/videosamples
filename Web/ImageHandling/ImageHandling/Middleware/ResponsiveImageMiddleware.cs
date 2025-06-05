namespace ImageHandling.Middleware
{
    public class ResponsiveImageMiddleware : IMiddleware
    {
        private readonly IWebHostEnvironment environment;

        public ResponsiveImageMiddleware(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Path.StartsWithSegments("/responsive-imageH", out var remaining))
            {
                await next(context);
                return;
            }

            string imageName = remaining.Value.Trim('/');
            string deviceType = context.Request.Query["size"].ToString().ToLower();

            if (string.IsNullOrEmpty(deviceType))
            {
                string userAgent = context.Request.Headers["User-Agent"].ToString().ToLower();

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
            string subfolder = deviceType switch
            {
                "mobile" => "mobile",
                "tablet" => "tablet",
                "desktop" => "desktop",
                _ => "desktop"
            };

            string imagePath = Path.Combine(this.environment.WebRootPath, "images", subfolder, imageName);
            if (!File.Exists(imagePath))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Image not found.");
                return;
            }

            context.Response.ContentType = GetMimeType(imagePath);
            await context.Response.SendFileAsync(imagePath);
        }

        private static string GetMimeType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
