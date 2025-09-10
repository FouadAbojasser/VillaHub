using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VillaHub.Web.ViewModels.Identity
{
    public class RegisterVM
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Name", ResourceType = typeof(Resources.SharedResources))]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
        [Display(Name = "Email", ResourceType = typeof(Resources.SharedResources))]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources.SharedResources))]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [Compare(nameof(Password), ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "PasswordMismatch")]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resources.SharedResources))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(12)]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "PhoneNumber", ResourceType = typeof(Resources.SharedResources))]
        [RegularExpression(@"^\d+$", ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "PhoneNumberDigitsOnly")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "SelectCountry")]
        [Display(Name = "Country", ResourceType = typeof(Resources.SharedResources))]
        public string Country { get; set; } = string.Empty;

        public List<SelectListItem>? CountryList { get; set; }

        [ValidateNever]
        public string RedirectUrl { get; set; } = string.Empty;

        public string? Role { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }
}
