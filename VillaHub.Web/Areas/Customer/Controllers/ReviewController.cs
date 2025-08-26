using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;
using VillaHub.Infrastructure.Migrations;
using VillaHub.Web.ViewModels.ContactUs;
using VillaHub.Web.ViewModels.Home;
using VillaHub.Web.ViewModels.Review;

namespace VillaHub.Web.Areas.Customer.Controllers
{
    [Area ("Customer")]
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
        }



        public IActionResult Index()
        {
            return View();
        }





        public IActionResult FloorReview(int villageId, int villaId, int floorNumber)
        {
            var FloorInDB = _unitOfWork.Floor.GetOne(
                f => f.FloorNumber == floorNumber &&
                     f.VillaId == villaId &&
                     f.VillageId == villageId,

            include: q => q
                .Include(f => f.Villa)
                .Include(f => f.Images)
                .Include(f => f.Amenities)
                .Include(f => f.Reviews)
                    .ThenInclude(r => r.User),

             noTracking: false);

            if (FloorInDB is not null)
            {
                //FloorReviewVM floorReviewVM = new()
                //{
                //    Floor = FloorInDB,
                //};
                return View(FloorInDB);
            }

            return BadRequest();
        }




        public async Task<IActionResult> AddCustomerReviewAsync (int FloorNumber,int VillaId, int VillageId, string UserId, string reviewText, int ratingValue)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                var applicationUser = await _userManager.FindByIdAsync(UserId);

                //Check either to update old comment or add new one
                var CommentInDb = _unitOfWork.Review.GetOne(r => r.FloorNumber == FloorNumber
                                                         && r.FloorVillaId == VillaId
                                                         && r.FloorVillageId == VillageId
                                                         && r.UserId == UserId);
                //Update Old Comment
                if (CommentInDb is not null)
                {
                    CommentInDb.Comment = reviewText;
                    CommentInDb.Rate = ratingValue;
                    CommentInDb.UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
                    CommentInDb.User = applicationUser;

                    _unitOfWork.Review.Update(CommentInDb);

                    //Send RealTime notification to SuperAdmin
                    await _notificationService.NotifyNewComment(CommentInDb);
                }
                else
                //Add New Comment
                {
                    if (applicationUser != null)
                    {
                        var userReviw = new Review()
                        {
                            Comment = reviewText,
                            Rate = ratingValue,
                            UserId = applicationUser.Id,
                            User = applicationUser,
                            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                            FloorNumber = FloorNumber,
                            FloorVillaId = VillaId,
                            FloorVillageId = VillageId,
                            UserName = applicationUser.Name,

                        };

                        await _unitOfWork.Review.CreateAsync(userReviw);

                        //Send RealTime notification to SuperAdmin
                        await _notificationService.NotifyNewComment(userReviw);
                    }
                }

                await _unitOfWork.Review.CommitAsync();

               
            }

            return PartialView("_ThankYouMessage");
        }




    }
}
