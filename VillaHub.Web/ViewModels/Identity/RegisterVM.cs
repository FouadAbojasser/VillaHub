using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VillaHub.Web.ViewModels.Identity
{
    public class RegisterVM
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        [Display(Name="Phone Number")]
        public string PhoneNumber {  get; set; }= string.Empty;
        [Required(ErrorMessage = "Please select a country")]
        public string Country { get; set; } = string.Empty;
        public List<SelectListItem>? CountryList { get; set; }
        [ValidateNever]
        public string RedirectUrl { get; set; } = string.Empty;
        public string? Role { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }
}
