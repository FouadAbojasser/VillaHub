using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Infrastructure.Repository;
using VillaHub.Web.Models;
using VillaHub.Web.ViewModels.Home;

namespace VillaHub.Web.Controllers
{
    //[Area("Customer")]
    public class HomeController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
           _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var villageList = _unitOfWork.Village.Get(null, [e => e.Villas]);

            return View(villageList);
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
