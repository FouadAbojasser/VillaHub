using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
using VillaHub.Web.ViewModels.Floor;
using VillaHub.Web.ViewModels.Villa;

namespace VillaHub.Web.Controllers
{
    public class FloorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public FloorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        public IActionResult Index()
        {
            var floorList = _unitOfWork.Floor.Get(
                null,
                [f => f.Villa!, f=>f.Villa.Village, f=>f.Images, f=>f.Amenities]
                );

            foreach (var floor in floorList)
            {
                var bookings = _unitOfWork.Booking.Get(b => b.FloorNumber == floor.FloorNumber && b.VillaId == floor.VillaId && b.VillageId == floor.VillageId);
                
                floor.BookingsCount = bookings.Count();

                floor.BookingsByStatus = new Dictionary<string, int>
                {
                    { SD.StatusPending,   bookings.Count(b => b.Status == SD.StatusPending) },
                    { SD.StatusApproved,  bookings.Count(b => b.Status == SD.StatusApproved) },
                    { SD.StatusCheckedIn, bookings.Count(b => b.Status == SD.StatusCheckedIn) },
                    { SD.StatusCompleted, bookings.Count(b => b.Status == SD.StatusCompleted) },
                    { SD.StatusCancelled, bookings.Count(b => b.Status == SD.StatusCancelled) }
                };
            }
            return View(floorList);
        }



        public IActionResult Create(int? id)
        {
            if (id is not null)
            {
                var amenityList = _unitOfWork.Amenity.Get(a => a.Type == Amenity.AmenityType.Floor);

                var selecetedVilla = _unitOfWork.Villa.GetOne(e => e.Id == id, [e => e.Village]);

                if (selecetedVilla is not null) 
                { 
                    var createModel = new FloorWithVillasVM
                    {
                        Villa = selecetedVilla,

                        Village = selecetedVilla.Village,

                        Amenities = amenityList.ToList(),
                    };
                   
                   return View(createModel);
                }
                
            }

            return BadRequest();
        } //not used


        [HttpPost]
        public async Task<IActionResult> CreateAsync(FloorWithVillasVM floorWithVillas, List<IFormFile> FloorImages)
        {
            var FloorInDb = _unitOfWork.Floor.GetOne(
                            f => f.FloorNumber == floorWithVillas.Floor!.FloorNumber
                            &&
                            f.VillaId == floorWithVillas.Floor.VillaId
                            &&
                            f.VillageId == floorWithVillas.Floor.VillageId,
                            [v => v.Village, m => m.Villa, g => g.Images],
                            false);

            ModelState.Remove("Villa.Village");

            if (!ModelState.IsValid)
            {
                return View(floorWithVillas);
            }

            if (FloorInDb is not null)
            {
                ModelState.AddModelError(string.Empty, $"Floor {FloorInDb.FloorNumber} in Villa {FloorInDb.Villa.Name} in Village {FloorInDb.Village.Name} Already Exist!");

                return View(floorWithVillas);
            }

            if (floorWithVillas.Floor is not null)
            {
                foreach (var x in floorWithVillas.SelectedAmenityIds)
                {
                    var amenityToAdd = _unitOfWork.Amenity.GetOne(e => e.Id == x, null, false);

                    if (amenityToAdd is not null)
                    {
                        floorWithVillas.Floor.Amenities.Add(amenityToAdd);
                        floorWithVillas.Floor.Price += amenityToAdd.Price;
                    }
                }

                floorWithVillas.Floor.CreateDate = DateTime.UtcNow;

                await _unitOfWork.Floor.CreateAsync(floorWithVillas.Floor);
            }

            

            // Handle images (same as before)
            foreach (var image in FloorImages)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "floors");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    floorWithVillas.Floor!.Images.Add(new Image
                    {
                        Name = fileName,
                        Type = "FloorImage",
                        Url = "/images/floors/" + fileName,
                        CreateDate = DateTime.UtcNow,
                        FloorNumber = floorWithVillas.Floor.FloorNumber,
                        FloorVillaId = floorWithVillas.Floor.VillaId,
                        FloorVillageId = floorWithVillas.Floor.VillageId
                    });

                   
                }
            }

            await _unitOfWork.Floor.CommitAsync();

            TempData["success"] = "Floor Created Successfully!";

            return RedirectToAction(nameof(Index));
        }





        public IActionResult Update(int floorNumber, int villaId, int villageId)
        {
            var amenityList = _unitOfWork.Amenity.Get(a=>a.Type == Amenity.AmenityType.Floor);

            var floorInDb = _unitOfWork.Floor.GetOne(
                e => e.FloorNumber == floorNumber
                &&
                e.VillaId == villaId
                &&
                e.VillageId == villageId,
                [v => v.Village, m => m.Villa, e => e.Images, m=>m.Amenities]);

            if (floorInDb != null)
            {
                var floorsWithVillas = new FloorWithVillasVM
                {
                    Village = floorInDb.Village,
                    Villa = floorInDb.Villa,
                    Floor = floorInDb,
                    Amenities = amenityList.ToList(), //All Amenities in Db
                   
                };

                return View(floorsWithVillas);
            }

            return RedirectToAction("Error", "Home");
        }




        [HttpPost]
        public async Task<IActionResult> UpdateAsync(FloorWithVillasVM floorWithVillasVM, List<IFormFile>? newFloorImages, string removeFloorImages)
        {
            
            var removedImagesList = JsonConvert.DeserializeObject<List<string>>(removeFloorImages ?? "[]");

            var FloorInDb = _unitOfWork.Floor.GetOne(
                            f => f.FloorNumber == floorWithVillasVM.Floor!.FloorNumber
                            &&
                            f.VillaId == floorWithVillasVM.Villa!.Id
                            &&
                            f.VillageId == floorWithVillasVM.Village!.Id,
                            [m => m.Villa, g => g.Images, a=>a.Amenities],
                            false);


            if (FloorInDb is null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }
            if (floorWithVillasVM.Floor is not null)
            {
                // Update FloorInDb with new values from the FloorWithVillasVM
                FloorInDb.Description = floorWithVillasVM.Floor.Description;
                FloorInDb.Price = floorWithVillasVM.Floor.Price;
                FloorInDb.Area = floorWithVillasVM.Floor.Area;
                FloorInDb.Capacity = floorWithVillasVM.Floor.Capacity;

                List<Amenity> AmenitiesToAdd = [];
                List<Amenity> AmenitiesToRemove = [];

                foreach (var selectedAmenityId in floorWithVillasVM.SelectedAmenityIds)
                {
                    if(!FloorInDb.Amenities.ToList().Any(a=>a.Id == selectedAmenityId))
                    {
                        var amenityToAdd = _unitOfWork.Amenity.GetOne(e => e.Id == selectedAmenityId, null, false);

                        if (amenityToAdd is not null)
                        {
                            AmenitiesToAdd.Add(amenityToAdd);
                        }
                    }
              
                }


                foreach (var amenityInDb in FloorInDb.Amenities.Select(a=>a.Id))
                {
                    if (!floorWithVillasVM.SelectedAmenityIds.Any(a => a == amenityInDb))
                    {
                        var amenityToRemove = _unitOfWork.Amenity.GetOne(e => e.Id == amenityInDb, null, false);

                        if (amenityToRemove is not null)
                        {
                            AmenitiesToRemove.Add(amenityToRemove);
                        }
                    }
                   
                }


                foreach(var amenity in AmenitiesToAdd)
                {
                    FloorInDb.Amenities.Add(amenity);
                    FloorInDb.Price += amenity.Price;
                }
                foreach (var amenity in AmenitiesToRemove)
                {
                    FloorInDb.Amenities.Remove(amenity);
                    FloorInDb.Price -= amenity.Price;
                }

                FloorInDb.UpdateDate = DateTime.UtcNow;
            }

            // Handling Sliders
            await HandlingUpdateVillaImagesAsync(FloorInDb, removedImagesList, newFloorImages);

            await _unitOfWork.Floor.CommitAsync();

            TempData["success"] = "Floor Updated Successfully";

            return RedirectToAction(nameof(Index));

        }

        private async Task HandlingUpdateVillaImagesAsync(Floor floorInDb, List<string>? removedImagesList, List<IFormFile>? newVillaImages)
        {

            // Handle removed images
            if (removedImagesList != null && removedImagesList.Count > 0)
            {
                foreach (var imageUrl in removedImagesList)
                {
                    string fileName = Path.GetFileName(imageUrl);

                    var imageToRemove = floorInDb.Images.FirstOrDefault(i => i.Name == fileName);

                    if (imageToRemove != null)
                    {
                        floorInDb.Images.Remove(imageToRemove);

                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "floors");

                        string filePath = Path.Combine(folderPath, fileName);

                        try
                        {
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file: {ex.Message}");
                        }

                        _unitOfWork.Image.Delete(imageToRemove);
                    }
                }
            }

            // Handle new images
            if (newVillaImages != null && newVillaImages.Count > 0)
            {
                foreach (var image in newVillaImages)
                {
                    if (image != null && image.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "floors");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        floorInDb.Images.Add(new Image
                        {
                            Name = fileName,
                            Type = "FloorImage",
                            Url = filePath,
                            CreateDate = DateTime.UtcNow

                        });
                    }
                }
            }
        }



        public IActionResult Delete(int floorNumber, int villaId, int villageId)
        {
            var floorInDb = _unitOfWork.Floor.GetOne(
                e => e.FloorNumber == floorNumber
                && e.VillaId == villaId
                && e.VillageId == villageId
                && e.IsDeleted == false,
                [v => v.Village, m => m.Villa, e => e.Images]);

            if (floorInDb is not null)
            {
                return View(floorInDb);
            }

            return RedirectToAction("Error", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Floor floor)
        {

            var floorInDb = _unitOfWork.Floor.GetOne(
                a => a.FloorNumber == floor.FloorNumber
                && a.VillaId == floor.VillaId
                && a.VillageId == floor.VillageId
                && a.IsDeleted == false, [m => m.Images], true);

            if (floorInDb is not null)
            {
                var hasActiveBookings = _unitOfWork.Booking.Get(b =>
                    b.FloorNumber == floorInDb.FloorNumber &&
                    b.VillaId == floorInDb.VillaId &&
                    b.VillageId == floorInDb.VillageId &&
                    (b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn)
                ).Any();

                if (hasActiveBookings)
                {
                    TempData["error"] = "Cannot delete floor with approved or checked-in bookings.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (floorInDb is null)
            {
                return RedirectToAction("Error", "Home");
            }

            // Delete associated images from the server
            if (floorInDb.Images is not null && floorInDb.Images.Count > 0)
            {
                foreach (var image in floorInDb.Images)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "floors");

                    string oldFilePath = Path.Combine(folderPath, image.Name!);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        Console.WriteLine($"Error deleting file: {ex.Message}");
                    }
                    _unitOfWork.Image.Delete(image);
                }
                await _unitOfWork.Image.CommitAsync();
            }

            //_unitOfWork.Floor.Delete(floorInDb);
           
            floorInDb.IsDeleted = true;
            _unitOfWork.Floor.Update(floorInDb);
            await _unitOfWork.Floor.CommitAsync();
            
            TempData["success"] = "Floor Soft-Deleted Successfully";

            return RedirectToAction(nameof(Index));

        }

       
        public async Task<IActionResult> RestoreAsync(int floorNumber, int villaId, int villageId)
        {
            var floorInDb = _unitOfWork.Floor.GetOne(
                   e=>e.FloorNumber==floorNumber
                && e.VillaId == villaId
                && e.VillageId == villageId
                && e.IsDeleted==true,null,true);

            if(floorInDb is null)
            {
                return NotFound();
            }

            floorInDb.IsDeleted=false;
            _unitOfWork.Floor.Update(floorInDb);
            await _unitOfWork.Floor.CommitAsync();

            TempData["success"] = "Floor Data has been restored successfully!";

            return RedirectToAction(nameof(Index));
        }




    }
    
}
