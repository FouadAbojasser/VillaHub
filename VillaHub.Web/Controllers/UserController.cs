using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
using VillaHub.Web.ViewModels.Identity;
using VillaHub.Web.ViewModels.User;

namespace VillaHub.Web.Controllers
{
    [Authorize(Roles = SD.Role_SuperAdmin)]
    public class UserController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> UserDetailsAsync(string Id)
        {

            var applicationUser = await _userManager.FindByIdAsync(Id);

            if (applicationUser != null)
            {
                var listOfRoles = await _userManager.GetRolesAsync(applicationUser);

                var userBookings = _unitOfWork.Booking.Get(b => b.UserId == Id);

                var logins = await _userManager.GetLoginsAsync(applicationUser);

                UserWithBookingsWithRoles userWithBookingsWithRoles = new()
                {
                    AppUser = applicationUser,
                    Bookings = userBookings.ToList(),
                    UserRoles = listOfRoles.ToList(),
                    ExternalLogins = logins.Select(l => l.LoginProvider).ToList()
                };

               return View(userWithBookingsWithRoles);
            }

            return NotFound();
        }


        public async Task<IActionResult> DeleteAsync(string Id)
        {
            var applicationUser = await _userManager.FindByIdAsync(Id);

            if (applicationUser != null)
            {
                var listOfRoles = await _userManager.GetRolesAsync(applicationUser);
                
                var logins = await _userManager.GetLoginsAsync(applicationUser);

                UserWithBookingsWithRoles userWithBookingsWithRoles = new()
                {
                    AppUser = applicationUser,
                    UserRoles = listOfRoles.ToList(),
                    ExternalLogins = logins.Select(l => l.LoginProvider).ToList()
                };

                return View(userWithBookingsWithRoles);
            }

            return NotFound();
           
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync(ApplicationUser applicationUser)
        {
            var applicationUserInDB = await _userManager.FindByIdAsync(applicationUser.Id);

            if (applicationUserInDB is null)
            {
                return RedirectToAction("Error", "Home");
            }


            if (!string.IsNullOrEmpty(applicationUserInDB.ImageUrl))
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");

                string oldFilePath = Path.Combine(folderPath, applicationUserInDB.ImageUrl);
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

                applicationUserInDB.ImageUrl = string.Empty;
            }

            //var result = await _userManager.DeleteAsync(applicationUserInDB);

            applicationUserInDB.Status = SD.Deleted_User;
            applicationUserInDB.DeletedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(applicationUserInDB);

            TempData["success"] = "User soft-deleted successfully.";

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> RestoreAsync(string id)
        {
            var applicationUserInDB= await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (applicationUserInDB is null)
            {
                return NotFound();
            }
            applicationUserInDB.Status = SD.Active_User;
            applicationUserInDB.DeletedAt = default;
            await _userManager.UpdateAsync(applicationUserInDB);

            TempData["success"] = "User restored successfully.";

            return RedirectToAction("Index");
        }

        

        public async Task<IActionResult> UpdateAsync(string id)
        {
            var applicationUserInDB = await _userManager.FindByIdAsync(id);

            if (applicationUserInDB is null)
            {
                return RedirectToAction("Error", "Home");
            }

            var userRole = await _userManager.GetRolesAsync(applicationUserInDB);

            var logins = await _userManager.GetLoginsAsync(applicationUserInDB);

            EditUserVM editUserVM = new()
            {
                AppUser = applicationUserInDB,
                PhoneNumber = applicationUserInDB.PhoneNumber,
                UserRole = userRole.FirstOrDefault(),
                ExternalLogins = logins.Select(l => l.LoginProvider).ToList(),
                RolesList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                CountryList = SD.CountryList.Select(c => new SelectListItem
                {
                    Text = c.Text,
                    Value = c.Text

                }).ToList()

            };

            return View(editUserVM);
        }





        [HttpPost]
        public async Task<IActionResult> UpdateAsync(EditUserVM user, IFormFile ProfileImage)
        {
            var applicationUserInDB = await _userManager.FindByIdAsync(user.AppUser.Id);
            
            ModelState.Remove("ProfileImage");

            if (ModelState.IsValid && applicationUserInDB != null)
            {
               
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");

                // If new image is uploaded
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);

                    string newFilePath = Path.Combine(folderPath, newFileName);

                    // Ensure directory exists
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Save new file
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    // Delete old file if it exists
                    if (!string.IsNullOrEmpty(applicationUserInDB.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(folderPath, applicationUserInDB.ImageUrl);
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

                    applicationUserInDB.ImageUrl = newFileName; // Update to new image
                }
                else
                {
                    // No new image uploaded, retain old image
                    user.AppUser.ImageUrl = applicationUserInDB.ImageUrl;
                }

                applicationUserInDB.Name = user.AppUser.Name;
                applicationUserInDB.Country = user.AppUser.Country;
                applicationUserInDB.PhoneNumber = user.PhoneNumber;

                var currentUserRole = await _userManager.GetRolesAsync(applicationUserInDB);

                if (currentUserRole != null) 
                {
                    await _userManager.RemoveFromRolesAsync(applicationUserInDB, currentUserRole);
                }

                if (user.UserRole != null)
                {
                    await _userManager.AddToRoleAsync(applicationUserInDB, user.UserRole);
                }

                await _userManager.UpdateAsync(applicationUserInDB);

                TempData["success"] = "User Updated Successfully";

                return RedirectToAction(nameof(Index));
            }

            EditUserVM editUserVMReturn = new();

            if (applicationUserInDB is not null)
            {
               var currentUserRole = await _userManager.GetRolesAsync(applicationUserInDB);
                editUserVMReturn.AppUser = applicationUserInDB;
                editUserVMReturn.UserRole= currentUserRole.FirstOrDefault();
                editUserVMReturn.Country = applicationUserInDB.Country;
                editUserVMReturn.RolesList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                });
                editUserVMReturn.CountryList = SD.CountryList.Select(c => new SelectListItem
                {
                    Text = c.Text,
                    Value = c.Text

                }).ToList();
            }
            
            return View(editUserVMReturn);
            
        }












        #region API Calls

        [HttpGet]
        [Authorize]
        public IActionResult GetAllUsers()
        {
            var allUsers = _userManager.Users.Select(u => new UserDataVM
            {
                Id = u.Id,
                ImageUrl = u.ImageUrl,
                Name = u.Name,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber!,
                Country = u.Country,
                CreatedAt = u.CreatedAt,
                Status = u.Status,
                DeletedAt = u.DeletedAt,
                                
            }).ToList();

            return Json( new {data=allUsers});
        }
        #endregion
    }
}
