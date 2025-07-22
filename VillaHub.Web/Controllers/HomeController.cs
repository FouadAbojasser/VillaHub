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
            var villaList = _unitOfWork.Villa.Get(null, [e => e.Images, e=>e.Floors, e=>e.Amenities]);
            var floorList = _unitOfWork.Floor.Get(null, [e => e.Village, e => e.Villa, e => e.Images, e =>e.Amenities, e=>e.Reviews]);

            HomeVM homeVM = new HomeVM()
            {
                Villages = villageList,
                Villas= villaList,
                Floors= floorList,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                PriceRange = 100,
                NumberOfNights = 1
            };

           return View(homeVM);
        }



        [HttpPost]
        //Not used 
        public IActionResult GetVillagesByDate(HomeVM homeVM)
        {
            Thread.Sleep(1000);

            var floorList = _unitOfWork.Floor.Get(null, [e => e.Village, e => e.Villa, e => e.Images, e => e.Amenities]);

            foreach (var floor in floorList)
            {
                if (floor.FloorNumber % 2 == 0)
                {
                    floor.isAvailable = false;
                }
            }

            HomeVM returnHomeVM = new HomeVM()
            {
                Floors = floorList,
                CheckInDate= homeVM.CheckInDate,
                PriceRange= homeVM.PriceRange,
                NumberOfNights= homeVM.NumberOfNights
            };

            return PartialView("_FloorList",returnHomeVM);
        }



        //Logic for Floor Availability
        public IActionResult CheckFloorAvailability(int floorNumber, int villaId, int villageId, HomeVM homeVM)
        {
            Thread.Sleep(1000);

            //Get All floors
            var floorList = _unitOfWork.Floor.Get(null, [e => e.Village, e => e.Villa, e => e.Images, e => e.Amenities]);

            //Get All Bookings with status "Approved"
            var AllBookings = _unitOfWork.Booking.Get(b => b.Status == SD.StatusApproved);

            foreach (var booking in AllBookings) 
            {
                var bookCheckIn = booking.CheckInDate;
                var bookCheckOut = booking.CheckOutDate;

                var searchCheckIn = homeVM.CheckInDate;
                var searchCheckOut = homeVM.CheckInDate.AddDays(homeVM.NumberOfNights);

                bool isOverlapping = bookCheckIn <= searchCheckOut && searchCheckIn <= bookCheckOut;

                foreach (var floor in floorList)
                {
                    if (isOverlapping == true && floor.FloorNumber == booking.FloorNumber && floor.VillaId == booking.VillaId && floor.VillageId == booking.VillageId )
                    {
                        floor.isAvailable = false;
                    }
                }
            }

            HomeVM returnHomeVM = new HomeVM()
            {
                Floors = floorList,
                CheckInDate = homeVM.CheckInDate,
                PriceRange = homeVM.PriceRange,
                NumberOfNights = homeVM.NumberOfNights
            };

            return PartialView("_FloorList", returnHomeVM);

           
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
