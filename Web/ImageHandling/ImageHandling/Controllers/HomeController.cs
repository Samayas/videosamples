using System.Diagnostics;
using ImageHandling.ViewModels;
using ImageHandling.ViewModels.Home;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ImageHandling.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IWebHostEnvironment environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            this.logger = logger;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageTypes()
        {
            return View(new HomeViewModel());
        }

        public IActionResult Image1LMS3Versions()
        {
            return View(new HomeViewModel());
        }

        public IActionResult Image2LMSResponsiveBootstrap()
        {
            return View(new HomeViewModel());
        }

        public IActionResult Image3LMSPictureMedia()
        {
            return View(new HomeViewModel());
        }

        public IActionResult Image4LMS3DataAttributes()
        {
            return View(new HomeViewModel());
        }

        public IActionResult Image5LMSSrcSetAttribute()
        {
            return View(new HomeViewModel());
        }
      
        public IActionResult ImageS1LMSImageController()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageS2LMSImageControllerV2()
        {
            return View(new HomeViewModel());
        }
        
        public IActionResult ImageS3LMSImageAdvancedController()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageM1LMSImageControllerV3()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageM2LMSImageControllerV3()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageM3LMSImageControllerV3()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageM4LMSImageControllerV3()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageM5LMSPictureMediaControllerV3()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageM6LMSImageMediaControllerV4()
        {
            return View(new HomeViewModel());
        }

        public IActionResult ImageH1LMSPictureMediaHandlerV1()
        {
            return View(new HomeViewModel());
        }

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ErrorViewModel viewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                IsDevelopment = this.environment.IsDevelopment()
            };

            IExceptionHandlerPathFeature? exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            IStatusCodeReExecuteFeature? statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            if (exceptionHandlerFeature != null)
            {
                // An unhandled exception occurred.
                Exception exception = exceptionHandlerFeature.Error;
                viewModel.StatusCode = HttpContext.Response.StatusCode; 
                viewModel.ErrorMessage = exception?.Message ?? "An unexpected server error occurred.";
                viewModel.OriginalPath = exceptionHandlerFeature.Path; 
                viewModel.ExceptionType = exception?.GetType().Name;

                if (viewModel.IsDevelopment && exception != null)
                {
                    viewModel.StackTrace = exception.StackTrace;
                }

                this.logger.LogError(exception, "Unhandled exception caught by ExceptionHandler. Path: {OriginalPath}, RequestId: {RequestId}", viewModel.OriginalPath, viewModel.RequestId);
            }
            else if (statusCodeReExecuteFeature != null)
            {
                viewModel.StatusCode = HttpContext.Response.StatusCode; 
                viewModel.OriginalPath = statusCodeReExecuteFeature.OriginalPathBase + statusCodeReExecuteFeature.OriginalPath + statusCodeReExecuteFeature.OriginalQueryString;

                switch (viewModel.StatusCode)
                {
                    case 400:
                        viewModel.ErrorMessage = "The server could not understand the request due to invalid syntax (Bad Request).";
                        break;
                    case 401:
                        viewModel.ErrorMessage = "Access is denied. You might need to log in to access this resource (Unauthorized).";
                        break;
                    case 403:
                        viewModel.ErrorMessage = "You do not have permission to access this resource (Forbidden).";
                        break;
                    case 404:
                        viewModel.ErrorMessage = $"The page or resource at '{viewModel.OriginalPath}' could not be found (Not Found).";
                        break;
                    case 500:
                        viewModel.ErrorMessage = "An unexpected error occurred on the server (Internal Server Error).";
                        break;
                    default:
                        viewModel.ErrorMessage = $"An error (Status Code: {viewModel.StatusCode}) occurred while processing your request for '{viewModel.OriginalPath}'.";
                        break;
                }

                this.logger.LogWarning("HTTP Error {StatusCode} processed by StatusCodePages. Original Path: {OriginalPath}, RequestId: {RequestId}", viewModel.StatusCode, viewModel.OriginalPath, viewModel.RequestId);
            }
            else
            {
                viewModel.StatusCode = (HttpContext.Response.StatusCode >= 400) ? HttpContext.Response.StatusCode : 500;
                viewModel.ErrorMessage = $"An unexpected error occurred (Status Code: {viewModel.StatusCode}).";
                viewModel.OriginalPath = HttpContext.Request.Path; // The path that led directly to this Error action.
                this.logger.LogWarning("Error action accessed directly or without specific error context. Path: {OriginalPath}, StatusCode: {StatusCode}, RequestId: {RequestId}", viewModel.OriginalPath, viewModel.StatusCode, viewModel.RequestId);
            }

            return View(viewModel);
        }
    }
}
