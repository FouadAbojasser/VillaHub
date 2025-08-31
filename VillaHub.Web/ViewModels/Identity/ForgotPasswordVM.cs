using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels.Identity
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
        [Display(Name = "UserNameorEmial", ResourceType = typeof(Resources.SharedResources))]
        public string UserNameOrEmail { get; set; } = string.Empty;
        public string ResetMethod { get; set; } = string.Empty;

    }
}
