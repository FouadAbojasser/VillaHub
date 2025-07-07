using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;
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
                [e => e.Village!]
                );
            return View(villaList);
        }

        public IActionResult Create()
        {

            var createModel = new VillaWithVillagesVM
            {

                Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(createModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(VillaWithVillagesVM villaWithVillage, IFormFile MainImg, List<IFormFile> VillaImages)
        {

            ModelState.Remove("Villa.Village");

            if (!ModelState.IsValid)
            {
                villaWithVillage.Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

                return View(villaWithVillage);
            }


            if (villaWithVillage.Villa is not null && MainImg is not null && MainImg.Length > 0)
            {
                //=> First Process the Image File 
                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(MainImg.FileName);

                // Define the folder path
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

                // Create folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Combine folder and filename to get full path
                var filePath = Path.Combine(folderPath, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await MainImg.CopyToAsync(stream);
                }

                // Save the image name to the database
                villaWithVillage.Villa.MainImg = fileName;

                //=> Second Save the Object to the Db
                await _unitOfWork.Villa.CreateAsync(villaWithVillage.Villa);


            }

            // Handling Multiple Images - Sliders
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

                    // Save the image name to the Villa.Images List
                    villaWithVillage.Villa!.Images.Add(new Image
                    {
                        Name = fileName,
                        Type = "VillaImage",
                        Url = filePath,
                        CreateDate = DateTime.UtcNow

                    });

                }

            }

            await _unitOfWork.Villa.CommitAsync();

            TempData["success"] = "Villa Created Successfully";

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Update(int id)
        {
            var villaInDb = _unitOfWork.Villa.GetOne(e => e.Id == id, [m => m.Village!, e => e.Images]);

            if (villaInDb != null)
            {
                var villaWithVillages = new VillaWithVillagesVM
                {
                    Villa = villaInDb,
                    Villages = _unitOfWork.Village.Get().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    })

                };

                return View(villaWithVillages);
            }
            return RedirectToAction("Error", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateAsync(VillaWithVillagesVM villaWithVillages, IFormFile newMainImg, string removeMainImg, List<IFormFile>? newVillaImages, string removeVillaImages)
        {

            var removedImagesList = JsonConvert.DeserializeObject<List<string>>(removeVillaImages ?? "[]");

            var VillaInDb = _unitOfWork.Villa.GetOne(
                v => v.Id == villaWithVillages.Villa!.Id, [m => m.Village, g => g.Images], false);

            if (VillaInDb is null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }
            if (villaWithVillages.Villa is not null)
            {
                // Update VillaInDb with new values from the VillaWithVillagesVM
                VillaInDb.Name = villaWithVillages.Villa.Name;
                VillaInDb.Description = villaWithVillages.Villa.Description;
                VillaInDb.NumberOfFloors = villaWithVillages.Villa.NumberOfFloors;
                VillaInDb.Area = villaWithVillages.Villa.Area;
                VillaInDb.Capacity = villaWithVillages.Villa.Capacity;
                VillaInDb.Latitude = villaWithVillages.Villa.Latitude;
                VillaInDb.Longitude = villaWithVillages.Villa.Longitude;
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

            // Handle removed images
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
                            Url = filePath,
                            CreateDate = DateTime.UtcNow

                        });
                    }
                }
            }
        }



        public IActionResult Delete(int id)
        {
            var villaInDb = _unitOfWork.Villa.GetOne(a => a.Id == id, [m=>m.Images]);

            if (villaInDb is not null)
            {
                return View(villaInDb);
            }

            return RedirectToAction("Error", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Villa villa)
        {

            var villaInDb = _unitOfWork.Villa.GetOne(a => a.Id == villa.Id, [m=>m.Images], true);

            if (villaInDb is null)
            {
                return RedirectToAction("Error", "Home");
            }

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
                    // Log exception
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                }
            }

            

            if (villaInDb.Images is not null && villaInDb.Images.Count > 0) 
            {
                foreach (var image in villaInDb.Images) 
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villas");

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

            _unitOfWork.Villa.Delete(villaInDb);

            await _unitOfWork.Villa.CommitAsync();

            TempData["success"] = "Villa Deleted Successfully";

            return RedirectToAction(nameof(Index));

        }











    }
}
