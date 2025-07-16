using System.ComponentModel.DataAnnotations;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.Home
{
    public class HomeVM
    {
        public IEnumerable<Village>? Villages { get; set; }
        [Display(Name = "Check In Date")]
        public DateOnly CheckInDate { get; set; } 
        [Display(Name = "Number of Nights")]
        public int? NumberOfNights { get; set; }
        [Display(Name = "Price Range")]
        public int? PriceRange { get; set; }
    }
}
