using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace VillaHub.Web.ViewModels.Identity
{
    public class LoginVM
    {
        [Required]
        [Display(Name = "User Name or Emial")]
        public string UserNameOrEmail { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
       
    }
}
