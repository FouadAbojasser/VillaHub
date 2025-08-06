using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.User
{
    public class EditUserVM
    {
        public ApplicationUser AppUser { get; set; } = null!;
        public List<string> ExternalLogins { get; set; } = [];
        public string Country { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? CountryList { get; set; }
        public string? UserRole { get; set; }
        public IEnumerable<SelectListItem>? RolesList { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }
    }
}
