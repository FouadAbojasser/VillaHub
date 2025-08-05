using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
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


        #region API Calls

        [HttpGet]
        [Authorize]
        public IActionResult GetAllUsers()
        {
            var allUsers = _userManager.Users.Select(u => new UserDataVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber!,
                Country = u.Country,
                CreatedAt = u.CreatedAt,

            }).ToList();

            return Json( new {data=allUsers});
        }
        #endregion
    }
}
