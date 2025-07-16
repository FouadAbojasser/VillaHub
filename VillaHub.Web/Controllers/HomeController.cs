using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
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
            var villageList = _unitOfWork.Village.Get(null, [e => e.Villas, e=>e.Floors]);

            HomeVM homeVM = new HomeVM()
            {
                Villages = villageList,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                PriceRange = 100,
                NumberOfNights = 1
            };

           return View(homeVM);
        }



        [HttpPost]
        public IActionResult GetVillagesByDate(HomeVM homeVM)
        {
            Thread.Sleep(1000);

            var villageList = _unitOfWork.Village.Get(null, [e => e.Villas, e => e.Floors]);

            foreach (var village in villageList)
            {
                if (village.Id % 2 == 0)
                {
                    village.isAvailable = false;
                }
            }

            HomeVM returnHomeVM = new HomeVM()
            {
                Villages = villageList,
                CheckInDate= homeVM.CheckInDate,
                PriceRange= homeVM.PriceRange,
                NumberOfNights= homeVM.NumberOfNights
            };

            return PartialView("_VillageList",returnHomeVM);
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
