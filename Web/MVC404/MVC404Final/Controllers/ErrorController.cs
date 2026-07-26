using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC404Step1.ViewModels.Error;

namespace MVC404Step1.Controllers
{
    public class ErrorController : Controller
    {
        private static readonly HashSet<string> ResourceExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".avif", ".bmp", ".css", ".csv", ".eot", ".gif", ".ico",
            ".jpeg", ".jpg", ".js", ".map", ".mp3", ".mp4", ".otf",
            ".pdf", ".png", ".svg", ".ttf", ".txt", ".webp", ".woff",
            ".woff2", ".xml"
        };

        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        public async System.Threading.Tasks.Task<IActionResult> Index(int? statusCode = null)
        {
            IStatusCodeReExecuteFeature? feature = this.HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            string originalPath = feature?.OriginalPath ?? this.HttpContext.Request.Path.Value ?? string.Empty;
            if (feature != null && statusCode == StatusCodes.Status404NotFound && IsResourceRequest(originalPath))
            {
                IStatusCodePagesFeature? statusCodePagesFeature = this.HttpContext.Features.Get<IStatusCodePagesFeature>();
                if (statusCodePagesFeature != null)
                {
                    statusCodePagesFeature.Enabled = false;
                }

                return this.NotFound();
            }

            ErrorViewModel errorViewModel = new ErrorViewModel();

            return View(errorViewModel);
        }

        public IActionResult Internal(string url)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();

            if (string.IsNullOrEmpty(url))
            {
                url = "/";
            }

            return View(errorViewModel);
        }

        private static bool IsResourceRequest(PathString path)
        {
            string extension = Path.GetExtension(path.Value ?? string.Empty);

            return ResourceExtensions.Contains(extension)
                || path.StartsWithSegments("/css")
                || path.StartsWithSegments("/images")
                || path.StartsWithSegments("/js")
                || path.StartsWithSegments("/lib");
        }
    }
}
