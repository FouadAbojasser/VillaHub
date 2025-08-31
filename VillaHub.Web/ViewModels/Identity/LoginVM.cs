using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels.Identity
{
    public class LoginVM
    {
        
        
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
        [Display(Name = "UserNameorEmial", ResourceType = typeof(Resources.SharedResources))]
        public string UserNameOrEmail { get; set; } = string.Empty;

        
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources.SharedResources))]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
       
    }
}
