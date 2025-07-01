using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Utility;
using VillaHub.Web.Models;

namespace VillaHub.Web.Controllers
{
    //[Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
