using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels.Identity
{
    public class ResendConfirmationEmailVM
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),ErrorMessageResourceName ="Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
        [Display(Name = "Email", ResourceType = typeof(Resources.SharedResources))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
    }
}
