using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        [Required]
        public string Country { get; set; } = string.Empty;
        public List<SelectListItem>? CountryList { get; set; }
    }
}
