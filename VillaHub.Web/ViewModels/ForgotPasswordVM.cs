using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [Display(Name ="User Name or Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;
        public string ResetMethod { get; set; } = string.Empty;

    }
}
