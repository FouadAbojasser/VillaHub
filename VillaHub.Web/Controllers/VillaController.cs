using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;
using VillaHub.Web.ViewModels.Floor;
using VillaHub.Web.ViewModels.Villa;

namespace VillaHub.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        public IActionResult Index()
        {
            var villaList = _unitOfWork.Villa.Get(
                null,
                [e => e.Village!, f => f.Floors, a=>a.Amenities],
                true,
                orderBy: q => q.OrderBy(v => v.Village.Name).ThenBy(v => v.Name)
                );
            return View(villaList);
        }



        public IActionResult Create(int? id)
        {
            var amenityList = _unitOfWork.Amenity.Get(a => a.Type == Amenity.AmenityType.Villa);

            var createModel = new VillaWithVillagesVM
            {

                Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                    Selected = (id != null && u.Id == id)  // pre-select if match
                }),

                Amenities = amenityList.ToList(), //All Amenities in Db
            };
            return View(createModel);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync(VillaWithVillagesVM villaWithVillage, IFormFile MainImg, List<IFormFile> VillaImages)
        {
            ModelState.Remove("Villa.Village");

            if (villaWithVillage.Villa is not null)
            {
                var checkDublicatedVillaName = _unitOfWork.Villa.GetOne(e => e.Name == villaWithVillage.Villa.Name);

                if (checkDublicatedVillaName is not null && checkDublicatedVillaName.VillageId == villaWithVillage.Villa.VillageId)
                {
                    villaWithVillage.Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });

                    ModelState.AddModelError(string.Empty, "Villa Name Already Exist");

                    return View(villaWithVillage);
                }
            }

            if (!ModelState.IsValid)
            {
                villaWithVillage.Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

                return View(villaWithVillage);
            }

            // === Handle Main Image
            if (villaWithVillage.Villa is not null && MainImg is not null && MainImg.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(MainImg.FileName);

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await MainImg.CopyToAsync(stream);
                }

                villaWithVillage.Villa.MainImg = fileName;

                // Add Amenities to the model.Villa
                foreach (var x in villaWithVillage.SelectedAmenityIds)
                {
                    var amenityToAdd = _unitOfWork.Amenity.GetOne(e => e.Id == x, null, false);

                    if (amenityToAdd is not null)
                    {
                        villaWithVillage.Villa.Amenities.Add(amenityToAdd);
                    }
                }

                // Save the villa to get the generated Id
                await _unitOfWork.Villa.CreateAsync(villaWithVillage.Villa);

                await _unitOfWork.Villa.CommitAsync(); 
            }

            // === Handle Gallery Images
            foreach (var image in VillaImages)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                   
                    var villaImage = new Image
                    {
                        Name = fileName,
                        Type = "VillaImage",
                        Url = "/images/villas/" + fileName,
                        CreateDate = DateTime.UtcNow,
                        VillaId = villaWithVillage.Villa!.Id,

                        //Set them to null as they are nullable and prevent setting them to 0
                        //ممكن نشيل هذا الجزء لكن الأفضل نخليه
                        FloorNumber = null,
                        FloorVillaId = null,
                        FloorVillageId = null
                    };


                    await _unitOfWork.Image.CreateAsync(villaImage);
                }
            }

            await _unitOfWork.Image.CommitAsync();

            TempData["success"] = "Villa Created Successfully";

            return RedirectToAction(nameof(Index));
        }



        public IActionResult Update(int id)
        {
            var amenityList = _unitOfWork.Amenity.Get(a => a.Type == Amenity.AmenityType.Villa);

            var villaInDb = _unitOfWork.Villa.GetOne(e => e.Id == id, [m => m.Village!, e => e.Images, a=>a.Amenities]);

            if (villaInDb != null)
            {
                var villaWithVillages = new VillaWithVillagesVM
                {
                    Villa = villaInDb,

                    Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }),

                    Amenities=amenityList.ToList()
                };

                return View(villaWithVillages);
            }
            return RedirectToAction("Error", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateAsync(VillaWithVillagesVM villaWithVillages, IFormFile newMainImg, string removeMainImg, List<IFormFile>? newVillaImages, string removeVillaImages)
        {
            var amenityList = _unitOfWork.Amenity.Get(a => a.Type == Amenity.AmenityType.Villa);

            var removedImagesList = JsonConvert.DeserializeObject<List<string>>(removeVillaImages ?? "[]");

            var VillaInDb = _unitOfWork.Villa.GetOne(
                v => v.Id == villaWithVillages.Villa!.Id, [m => m.Village, g => g.Images, a => a.Amenities], false);

            if (VillaInDb is null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }
            if (villaWithVillages.Villa is not null)
            {
                
                VillaInDb.Name = villaWithVillages.Villa.Name;
                VillaInDb.Description = villaWithVillages.Villa.Description;
                VillaInDb.NumberOfFloors = villaWithVillages.Villa.NumberOfFloors;
                VillaInDb.Area = villaWithVillages.Villa.Area;
                VillaInDb.Capacity = villaWithVillages.Villa.Capacity;
                VillaInDb.Latitude = villaWithVillages.Villa.Latitude;
                VillaInDb.Longitude = villaWithVillages.Villa.Longitude;

                foreach (var x in VillaInDb.Amenities.ToList())
                {
                    if (!villaWithVillages.SelectedAmenityIds.Any(a => a == x.Id))
                    {
                        VillaInDb.Amenities.Remove(x);
                    }
                }

                foreach (var x in villaWithVillages.SelectedAmenityIds)
                {
                    var amenityToAdd = _unitOfWork.Amenity.GetOne(e => e.Id == x, null, false);

                    if (amenityToAdd is not null)
                    {
                        VillaInDb.Amenities.Add(amenityToAdd);
                    }
                }

                VillaInDb.UpdateDate = DateTime.UtcNow;
            }

            // Handling Posters
            HandlingUpdateVillaMainImg(VillaInDb, removeMainImg, newMainImg!);

            // Handling Sliders
            await HandlingUpdateVillaImagesAsync(VillaInDb, removedImagesList, newVillaImages);

            await _unitOfWork.Villa.CommitAsync();

            TempData["success"] = "Villa Edited Successfully";

            return RedirectToAction(nameof(Index));

        }


        private void HandlingUpdateVillaMainImg(Villa villaInDb, string removeMainImg, IFormFile newMainImg)
        {
            // First handle removed images
            if (removeMainImg is not null && removeMainImg.Length != 0)
            {
                string fileName = Path.GetFileName(removeMainImg);

                // Delete physical file
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

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
                    // Log exception or notify admin
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                }

                
            }
    
            // Then handle new image uploads
            if (newMainImg is not null && newMainImg.Length != 0 )
            {
                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newMainImg.FileName);

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

                // Create folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Save the file
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    newMainImg.CopyTo(stream);
                }

                // Add to movie's images collection
                villaInDb.MainImg = fileName;
               
            }
        }


        private async Task HandlingUpdateVillaImagesAsync(Villa villaInDb, List<string>? removedImagesList, List<IFormFile>? newVillaImages)
        {
            //Handle removed images
            if (removedImagesList != null && removedImagesList.Count > 0)
            {
                foreach (var imageUrl in removedImagesList)
                {
                    string fileName = Path.GetFileName(imageUrl);

                    var imageToRemove = villaInDb.Images.FirstOrDefault(i => i.Name == fileName);

                    if (imageToRemove != null)
                    {
                        villaInDb.Images.Remove(imageToRemove);

                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

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

                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        villaInDb.Images.Add(new Image
                        {
                            Name = fileName,
                            Type = "VillaImage",
                            Url = "/images/villas/" + fileName,
                            CreateDate = DateTime.UtcNow,
                            VillaId = villaInDb.Id, 
                            
                            FloorNumber = null,
                            FloorVillaId = null,
                            FloorVillageId = null
                        });
                    }
                }
            }
        }



        public IActionResult Delete(int id)
        {
            var villaInDb = _unitOfWork.Villa.GetOne(a => a.Id == id, [m => m.Images]);

            if (villaInDb is not null)
            {
                return View(villaInDb);
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Villa villa)
        {
            var villaInDb = _unitOfWork.Villa.GetOne(a => a.Id == villa.Id, [m => m.Images], true);

            if (villaInDb is not null)
            {
                var hasRelatedFloors = _unitOfWork.Floor.Get(e => e.VillaId == villaInDb.Id);
                foreach (var floor in hasRelatedFloors)
                {
                    if (!floor.IsDeleted)
                    {
                        TempData["error"] = "Cannot delete villa. Related floors are not deleted.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            if (villaInDb is null)
            {
                return RedirectToAction("Error", "Home");
            }

            // === 🔻 Delete Main Image
            if (!string.IsNullOrEmpty(villaInDb.MainImg))
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

                string oldFilePath = Path.Combine(folderPath, villaInDb.MainImg);

                try
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                }
            }

            //التأكد أن مسح الصورة يتم لصور الفيلا فقط 
            if (villaInDb.Images is not null && villaInDb.Images.Count > 0)
            {
                foreach (var image in villaInDb.Images.ToList())
                {
                    // التأكد أولا أن الصورة لا تتبع لطابق 
                    if (image.FloorNumber is null && image.FloorVillaId is null && image.FloorVillageId is null)
                    {
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");
                        string oldFilePath = Path.Combine(folderPath, image.Name ?? "");

                        try
                        {
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file: {ex.Message}");
                        }

                        _unitOfWork.Image.Delete(image);
                    }
                }
            }

            //_unitOfWork.Villa.Delete(villaInDb);
            villaInDb.IsDeleted = true;
            _unitOfWork.Villa.Update(villaInDb);
            await _unitOfWork.Villa.CommitAsync();

            TempData["success"] = "Villa Deleted Successfully";

            return RedirectToAction(nameof(Index));
        }


    }
}
