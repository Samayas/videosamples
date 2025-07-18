using Microsoft.AspNetCore.Mvc;

namespace BlogSlugify3ConventionalTransformerSlugify.Controllers
{
    [Route("[controller]")]
    public class SubscriptionManagementController : Controller
    {
        [HttpGet("[action]")]
        public IActionResult ListAll()
        {
            return Ok("ok");
        }
    }
}
