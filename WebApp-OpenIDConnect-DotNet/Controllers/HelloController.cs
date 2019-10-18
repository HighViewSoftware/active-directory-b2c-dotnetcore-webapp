using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class HelloController : Controller
    {
        public IActionResult Index()
        {
            var id = User.Claims.SingleOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            return Json(new { Id = id });
        }
    }
}
