using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels
{
    public class ExternalLoginConfirmationVM
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
