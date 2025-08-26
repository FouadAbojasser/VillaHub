using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Utility;
using VillaHub.Web.ViewModels.ContactUs;

namespace VillaHub.Web.Areas.Customer.Controllers
{
    [Area("Customer")]


    public class ContactUsController : Controller
    {

        private readonly IEmailSender _emailSender;

        public ContactUsController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult ContactUsForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ContactUsFormAsync(ContactUsVM contactUs)
        {
            if(ModelState.IsValid)
            {
                await _emailSender.SendEmailAsync(
                    SD.AdminEmial,
                    $"Contact Us: {contactUs.Subject}",
                    $"<b>Email:</b> {contactUs.Email}<br/>" +
                    "<b></b>" + //empty line
                    $"<b>Message:</b><br/>{contactUs.Message}"
                );

                TempData["success"] = "Your message has been sent successfully!";

                return RedirectToAction("Index", "Home", new {Area=""});
            }

            return View(contactUs);
        }
    }
}
