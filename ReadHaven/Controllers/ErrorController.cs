using Microsoft.AspNetCore.Mvc;

namespace ReadHaven.Controllers
{
    [Route("[controller]")]
    public class ErrorController : BaseController
    {
        [Route("NotFound")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
