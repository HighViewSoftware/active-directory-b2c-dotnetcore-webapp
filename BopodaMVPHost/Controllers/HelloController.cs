using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace BopodaMVPHost.Controllers
{
    public class HelloController : Controller
    {
        public IActionResult Index()
        {
            var id = User.Claims.SingleOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            return Json(new
            {
                Id = id,
                Claims = User.Claims.Select(t => new
                {
                    t.Type,
                    t.Value
                })
            });
        }
    }
}
