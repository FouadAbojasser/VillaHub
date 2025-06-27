using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels
{
    public class ResendConfirmationEmailVM
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
