using Microsoft.AspNetCore.Mvc;

namespace BopodaMVPHost.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Json(new
            {
                Message = "Welcome to bopoda MVP API Server",
                User.Identity.IsAuthenticated
            });
        }
    }
}
