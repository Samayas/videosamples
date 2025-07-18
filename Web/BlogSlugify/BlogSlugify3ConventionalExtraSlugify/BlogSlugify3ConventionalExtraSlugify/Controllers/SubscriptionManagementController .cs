using Microsoft.AspNetCore.Mvc;

namespace BlogSlugify3ConventionalExtraSlugify.Controllers
{
    public class SubscriptionManagementController : Controller
    {
        public IActionResult ListAll()
        {
            return Ok("ok");
        }
    }
}
