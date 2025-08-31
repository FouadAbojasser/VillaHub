using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VillaHub.Web.ViewModels.Identity
{
    public class ExternalLoginConfirmationVM
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
        [Display(Name = "Email", ResourceType = typeof(Resources.SharedResources))]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [Display(Name = "PhoneNumber", ResourceType = typeof(Resources.SharedResources))]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "SelectCountry")]
        [Display(Name = "Country", ResourceType = typeof(Resources.SharedResources))]
        public string Country { get; set; } = string.Empty;
        public List<SelectListItem>? CountryList { get; set; }
    }
}
