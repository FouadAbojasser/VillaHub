using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Presentation;
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
                minPrice = 100,
                maxPrice= 200,
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
                minPrice = homeVM.minPrice,
                maxPrice=homeVM.maxPrice,
                NumberOfNights= homeVM.NumberOfNights
            };

            return PartialView("_FloorList",returnHomeVM);
        }



        //Logic for Floor Availability
        public IActionResult CheckFloorAvailability(int floorNumber, int villaId, int villageId, HomeVM homeVM)
        {
            Thread.Sleep(1000);

            //Get All floors
            var floorList = _unitOfWork.Floor.Get(null, [e => e.Village, e => e.Villa, e => e.Images, e => e.Amenities, e=>e.Reviews]);

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
                    if (isOverlapping == true && floor.FloorNumber == booking.FloorNumber && floor.VillaId == booking.VillaId && floor.VillageId == booking.VillageId)
                    {
                        floor.isAvailable = false;
                    }
                    if (floor.Price < homeVM.minPrice || floor.Price > homeVM.maxPrice)
                    {
                        floor.isInPriceRange = false;
                    }
                }
            }

            HomeVM returnHomeVM = new HomeVM()
            {
                Floors = floorList,
                CheckInDate = homeVM.CheckInDate,
                minPrice = homeVM.minPrice,
                maxPrice = homeVM.maxPrice,
                NumberOfNights = homeVM.NumberOfNights
            };

            return PartialView("_FloorList", returnHomeVM);

           
        }



        [HttpGet]
        public IActionResult GeneratePPTExport(int villageId, int villaId, int floorNumber)
        {
            var FloorInDb = _unitOfWork.Floor.GetOne(f=>f.FloorNumber==floorNumber
                                                  && f.VillaId==villaId
                                                  && f.VillageId==villageId,
                                                  [v=>v.Village, v=>v.Villa,v=>v.Images,v=>v.Amenities]);

            if (FloorInDb is null)
            {
                return RedirectToAction(nameof(Error));
            }

            //string basePath = _webHostEnvironment.WebRootPath;
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "ExportFloorDetails.pptx");
            //string filePath = basePath + @"/Exports/ExportVillaDetails.pptx";


            using IPresentation presentation = Presentation.Open(templatePath);

            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = $"Floor {FloorInDb.FloorNumber.ToString()} in Villa {FloorInDb.Villa.Name} in Village {FloorInDb.Village.Name}";
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = FloorInDb.Description;
            }


            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", FloorInDb.Capacity);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Floor Size: {0} sqft", FloorInDb.Area);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", FloorInDb.Price.ToString("C"));
            }


            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;

            if (shape is not null)
            {
                List<string> listItems = FloorInDb.Amenities.Select(x => x.Name).ToList();

                shape.TextBody.Text = "";

                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);

                }

            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;
            //if (shape is not null)
            //{
            //    byte[] imageData;
            //    string imageUrl;
            //    try
            //    {
            //        imageUrl = string.Format("{0}{1}", templatePath, FloorInDb.Images.FirstOrDefault().Name);
            //        imageData = System.IO.File.ReadAllBytes(imageUrl);
            //    }
            //    catch (Exception)
            //    {
            //        imageUrl = string.Format("{0}{1}", Directory.GetCurrentDirectory(), "/images/placeholder.png");
            //        imageData = System.IO.File.ReadAllBytes(imageUrl);
            //    }
            //    slide.Shapes.Remove(shape);
            //    using MemoryStream imageStream = new(imageData);
            //    IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);

            //}



            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", $"Floor-{FloorInDb.FloorNumber} Villa-{FloorInDb.Villa.Name} Village-{FloorInDb.Village.Name}.pptx");


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
