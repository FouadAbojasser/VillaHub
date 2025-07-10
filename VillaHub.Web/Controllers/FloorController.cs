using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using VillaHub.Application.Common.Interfaces;
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
                [e => e.Villa!, v=>v.Villa.Village, m=>m.Images]
                );
            return View(floorList);
        }



        public IActionResult Create(int? id)
        {
            if (id is not null)
            {
                var selecetedVilla = _unitOfWork.Villa.GetOne(e => e.Id == id, [e => e.Village]);

                if (selecetedVilla is not null) 
                { 
                    var createModel = new FloorWithVillasVM
                    {
                        Villa = selecetedVilla,

                        Village = selecetedVilla.Village
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

            TempData["success"] = "Floor Created Successfully";

            return RedirectToAction(nameof(Index));
        }





        public IActionResult Update(int floorNumber, int villaId, int villageId)
        {
            var floorInDb = _unitOfWork.Floor.GetOne(
                e => e.FloorNumber == floorNumber
                &&
                e.VillaId == villaId
                &&
                e.VillageId == villageId,
                [v => v.Village, m => m.Villa, e => e.Images]);

            if (floorInDb != null)
            {
                var floorsWithVillas = new FloorWithVillasVM
                {
                    Village = floorInDb.Village,
                    Villa = floorInDb.Villa,
                    Floor = floorInDb
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
                            [m => m.Villa, g => g.Images],
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
                FloorInDb.UpdateDate = DateTime.UtcNow;
            }

            // Handling Sliders
            await HandlingUpdateVillaImagesAsync(FloorInDb, removedImagesList, newFloorImages);

            await _unitOfWork.Villa.CommitAsync();

            TempData["success"] = "Villa Edited Successfully";

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
                &&
                e.VillaId == villaId
                &&
                e.VillageId == villageId,
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

            var floorInDb = _unitOfWork.Floor.GetOne(a => a.FloorNumber == floor.FloorNumber, [m => m.Images], true);

            if (floorInDb is null)
            {
                return RedirectToAction("Error", "Home");
            }

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
                }
            }

            _unitOfWork.Floor.Delete(floorInDb);

            await _unitOfWork.Floor.CommitAsync();

            TempData["success"] = "Floor Deleted Successfully";

            return RedirectToAction(nameof(Index));

        }
    }
}
