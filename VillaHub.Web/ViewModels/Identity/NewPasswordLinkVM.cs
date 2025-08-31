using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels.Identity
{
    public class NewPasswordLinkVM
    {
        public string ApplicationUserId { get; set; } = null!;


        public string Token { get; set; } = null!;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.SharedResources))]
        public string NewPassword { get; set; } = null!;


        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [Compare(nameof(NewPassword), ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "PasswordMismatch")]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resources.SharedResources))]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
