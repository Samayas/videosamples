using Microsoft.AspNetCore.Mvc;

namespace ModelizedCanonicald.Controllers
{
    public abstract class WebController<T> : Controller
    {
        private readonly ILogger<T> _logger;
        private readonly string _controllerName;
        private readonly string _controllerShortName;

        public WebController(ILogger<T> logger)
        {
            _controllerName = GetType().Name;
            _logger = logger;
            _controllerShortName = (_controllerName.EndsWith("Controller")) ? 
                _controllerName.Substring(0, _controllerName.Length - "Controller".Length) : 
                _controllerShortName = _controllerName;
        }

        public string SiteUrl
        {
            get { return "https://localhost:7078"; }
        }

        protected string GetActionName()
        {
            return base.ControllerContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
        }

        protected string ControllerShortName => _controllerShortName;

        protected string ControllerName => _controllerName;

    }
}
