using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.Controllers
{
    public class VillageController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillageController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var villageList = _unitOfWork.Village.Get(null, [v=>v.Villas]);

            return View(villageList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(Village village, IFormFile ImgUrl)
        {
            if (!ModelState.IsValid) 
            { 
                return View(village);
            }


            if (village != null && ImgUrl != null && ImgUrl.Length > 0)
            {
                //=> First Process the Image File 
                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImgUrl.FileName);

                // Define the folder path
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\villages");

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
                    await ImgUrl.CopyToAsync(stream);
                }

                // Save the image name to the database
                village.ImgUrl = fileName;

                //=> Second Save the Object to the Db
                await _unitOfWork.Village.CreateAsync(village);

                await _unitOfWork.Village.CommitAsync();

                TempData["success"] = "Village Created Successfully";
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Update(int id)
        {
            var village = _unitOfWork.Village.GetOne(e => e.Id == id);

            if (village is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(village);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateAsync(Village village, IFormFile ImgUrl)
        {
            var villageInDb = _unitOfWork.Village.GetOne(e => e.Id == village.Id, null);

            ModelState.Remove("ImgUrl");

            if (ModelState.IsValid && villageInDb != null)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villages");

                // If new image is uploaded
                if (ImgUrl != null && ImgUrl.Length > 0)
                {
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImgUrl.FileName);

                    string newFilePath = Path.Combine(folderPath, newFileName);

                    // Ensure directory exists
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Save new file
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await ImgUrl.CopyToAsync(stream);
                    }

                    // Delete old file if it exists
                    if (!string.IsNullOrEmpty(villageInDb.ImgUrl))
                    {
                        string oldFilePath = Path.Combine(folderPath, villageInDb.ImgUrl);
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

                    village.ImgUrl = newFileName; // Update to new image
                }
                else
                {
                    // No new image uploaded, retain old image
                    village.ImgUrl = villageInDb.ImgUrl;
                }

                _unitOfWork.Village.Update(village);

                await _unitOfWork.Village.CommitAsync();

                TempData["success"] = "Village Updated Successfully";

                return RedirectToAction(nameof(Index));
            }

            return View(village); // Return to edit form if model is invalid
        }



        public IActionResult Delete (int id)
        {
            var villageInDb = _unitOfWork.Village.GetOne(a => a.Id == id);

            if (villageInDb is not null)
            {
                return View(villageInDb);
            }

            return RedirectToAction("Error", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Village village)
        {

            var villageInDb = _unitOfWork.Village.GetOne(a => a.Id == village.Id,null,true);

            if (villageInDb is null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!string.IsNullOrEmpty(villageInDb.ImgUrl))
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "villages");

                string oldFilePath = Path.Combine(folderPath, villageInDb.ImgUrl);
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

            _unitOfWork.Village.Delete(villageInDb);

            await _unitOfWork.Village.CommitAsync();

            TempData["success"] = "Village Deleted Successfully";

            return RedirectToAction(nameof(Index));

        }











    }
}
