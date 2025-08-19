using System.ComponentModel.DataAnnotations;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.Home
{
    public class HomeVM
    {
        public IEnumerable<VillaHub.Domain.Entities.Floor>? Floors { get; set; }
        public IEnumerable<VillaHub.Domain.Entities.Villa>? Villas { get; set; }
        public IEnumerable<Village>? Villages { get; set; }
        [Display(Name = "Check In Date")]
        [Required(ErrorMessage = "Date must be greater than today.")]
        public DateOnly CheckInDate { get; set; } 
        [Display(Name = "Number of Nights")]
        public int NumberOfNights { get; set; }
        [Display(Name = "min Price / Night")]
        [Range(0, int.MaxValue, ErrorMessage = "Maximum price cannot be negative.")]
        public int minPrice{ get; set; }
        [Display(Name = "max Price / Night")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum price cannot be negative.")]
        public int maxPrice{ get; set; }
    }
}
