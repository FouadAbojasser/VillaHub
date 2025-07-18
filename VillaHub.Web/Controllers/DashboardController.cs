using Microsoft.AspNetCore.Mvc;

namespace VillaHub.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
