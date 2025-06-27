using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels
{
    public class NewPasswordLinkVM
    {
        public string ApplicationUserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="New Password")]
        public string NewPassword { get; set; } = null!;
        [Required]
        [Compare(nameof(NewPassword))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
