using System.ComponentModel.DataAnnotations;
using VillaHub.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VillaHub.Web.ViewModels.User
{
    public class UserDataVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime? DeletedAt { get; set; }
    }
}
