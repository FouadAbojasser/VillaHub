using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;

namespace VillaHub.Web.Controllers
{
    
    [Authorize(Roles = SD.Role_SuperAdmin)]
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReviewController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var AllReviews = _unitOfWork.Review.Get(null, [e=>e.Floor.Villa, e=>e.Floor.Village]);
           
            return View(AllReviews);
        }


        public async Task<IActionResult> AddReplyAsync (int Id, string ReplyText, string RepliedBy)
        {
            var userCommentInDB = _unitOfWork.Review.GetOne(e => e.Id == Id);

            if(userCommentInDB is null)
            {
                return BadRequest();
            }

            if(!string.IsNullOrEmpty(ReplyText) && !string.IsNullOrEmpty(RepliedBy))
            {
                userCommentInDB.Reply = ReplyText;
                userCommentInDB.ReplyUserId = RepliedBy;
                userCommentInDB.RepliedAt = DateOnly.FromDateTime(DateTime.UtcNow);

                _unitOfWork.Review.Update(userCommentInDB);
                await _unitOfWork.Review.CommitAsync();
            }
            

            return RedirectToAction(nameof(Index));
        }
    }
}
