using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.User
{
    public class UserWithBookingsWithRoles
    {
        public ApplicationUser AppUser { get; set; } = null!;
        public List<Booking> Bookings { get; set; } = [];
        public List<string> UserRoles { get; set; } = [];
        public List<string> ExternalLogins { get; set; } = [];
    }
}
