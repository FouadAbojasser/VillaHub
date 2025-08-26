using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels.ContactUs
{
    public class ContactUsVM
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        [MaxLength(200, ErrorMessage = "Maximum length is 200 characters!")]
        public string Message { get; set; } = string.Empty;
    }
}
