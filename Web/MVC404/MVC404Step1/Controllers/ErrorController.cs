using Microsoft.AspNetCore.Mvc;
using MVC404Step1.ViewModels.Error;

namespace MVC404Step1.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        public async System.Threading.Tasks.Task<IActionResult> Index(int? statusCode = null)
        {
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
    }
}
