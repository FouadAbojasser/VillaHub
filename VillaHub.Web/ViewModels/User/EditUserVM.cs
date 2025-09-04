using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.User
{
    public class EditUserVM
    {
        [Display(Name = "Email", ResourceType = typeof(Resources.SharedResources))]
        public ApplicationUser AppUser { get; set; } = null!;

        [Display(Name = "ExternalLogins", ResourceType =typeof(Resources.SharedResources))]
        public List<string> ExternalLogins { get; set; } = [];

        [Display(Name = "Country", ResourceType = typeof(Resources.SharedResources))]
        public string Country { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? CountryList { get; set; }


        public string? UserRole { get; set; }
        public IEnumerable<SelectListItem>? RolesList { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [Display(Name = "PhoneNumber", ResourceType = typeof(Resources.SharedResources))]
        public string? PhoneNumber { get; set; }
    }
}
