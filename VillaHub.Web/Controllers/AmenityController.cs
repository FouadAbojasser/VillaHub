using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.Controllers
{
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var amenitiesList = _unitOfWork.Amenity.Get();

            return View(amenitiesList);
        }


        public IActionResult Create()
        {
            ViewBag.TypeList = new SelectList(new[] { "Village", "Villa", "Floor" });

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync(Amenity amenity)
        {
            if (!ModelState.IsValid)
            {
                return View(amenity);

            }

            if (amenity is not null)
            {
                amenity.CreateDate = DateTime.UtcNow;

                await _unitOfWork.Amenity.CreateAsync(amenity);

                await _unitOfWork.Amenity.CommitAsync();

                TempData["success"] = "Amenity Created Successfully!";
            }

            return RedirectToAction(nameof(Index));

        }



        public IActionResult Update(int id)
        {
            var amenity = _unitOfWork.Amenity.GetOne(e => e.Id == id);

            ViewBag.TypeList = new SelectList(new[] { "Village", "Villa", "Floor" });

            if (amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenity);
        }





        [HttpPost]
        public async Task<IActionResult> UpdateAsync(Amenity amenity)
        {
            if (amenity is null)
            {
                return View(amenity);
            }

            var amenityInDb = _unitOfWork.Amenity.GetOne(e => e.Id == amenity.Id);

            if (amenityInDb is null)
            {
                return NotFound();
            }

            amenity.CreateDate = amenityInDb.CreateDate;

            ViewBag.TypeList = new SelectList(new[] { "Village", "Villa", "Floor" });
                
            var priceDiff = Math.Abs(amenityInDb.Price - amenity.Price);

            if (priceDiff != 0)
            {
                if (amenityInDb.Type.ToString() == "Floor")
                {
                    ICollection<Floor> floorsHaveThisAmenity = _unitOfWork.Floor.Get(e => e.Amenities.Any(e => e.Id == amenity.Id)).ToList();

                    foreach (var floor in floorsHaveThisAmenity)
                    {
                        if (amenityInDb.Price < amenity.Price)
                        {
                            floor.Price += priceDiff;
                        }
                        else
                        {
                            floor.Price -= priceDiff;
                        }
                        _unitOfWork.Floor.Update(floor);
                        await _unitOfWork.Floor.CommitAsync();
                    }
                }

            }

                amenity.CreateDate = amenityInDb.CreateDate;
                amenity.UpdateDate = DateTime.UtcNow;
                _unitOfWork.Amenity.Update(amenity);

                await _unitOfWork.Amenity.CommitAsync();

                TempData["success"] = "Amenity Updated Successfully";

                return RedirectToAction(nameof(Index));
                            
        }






        public IActionResult Delete(int id)
        {
            var amenityToDelete = _unitOfWork.Amenity.GetOne(e=>e.Id == id);

            if (amenityToDelete is not null)
            {
                return View(amenityToDelete);
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync(Amenity amenity)
        {
            var amenityInDb = _unitOfWork.Amenity.GetOne(a => a.Id == amenity.Id, null, true);

            if (amenityInDb is null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (amenityInDb is not null)
            {
                if (amenityInDb.Type.ToString() == "Floor")
                {
                  ICollection<Floor> floorsHaveThisAmenity =  _unitOfWork.Floor.Get(e => e.Amenities.Any(e => e.Id == amenity.Id)).ToList();

                    foreach (var floor in floorsHaveThisAmenity) 
                    {
                        floor.Price -= amenityInDb.Price;
                        _unitOfWork.Floor.Update(floor);
                        await _unitOfWork.Floor.CommitAsync();
                    }

                }

                _unitOfWork.Amenity.Delete(amenityInDb);

                await _unitOfWork.Amenity.CommitAsync();

                TempData["success"] = "Amenity Deleted Successfully!";

            }
           
            return RedirectToAction(nameof(Index));

        }









    }
}
