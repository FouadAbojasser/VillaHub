using System.ComponentModel.DataAnnotations;
using VillaHub.Domain.Entities;
using VillaHub.Web.Resources;

namespace VillaHub.Web.ViewModels.Home
{
    public class HomeVM
    {
        public IEnumerable<VillaHub.Domain.Entities.Floor>? Floors { get; set; }
        public IEnumerable<VillaHub.Domain.Entities.Villa>? Villas { get; set; }
        public IEnumerable<Village>? Villages { get; set; }


        [Display(Name = "CheckInDate", ResourceType = typeof(SharedResources))]
        [DataType(DataType.Date)] // tells the UI it's a date
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Date must be greater than today.")]
        public DateOnly CheckInDate { get; set; }


        [Display(Name = "NumberOfNights", ResourceType = typeof(SharedResources))]
        [Required]
        public int NumberOfNights { get; set; }

        [Display(Name = "MinPricePerNight",ResourceType =typeof(SharedResources))]
        [Range(0, int.MaxValue, ErrorMessage = "Maximum price cannot be negative.")]
        [Required]
        public int minPrice{ get; set; }

        [Display(Name = "MaxPricePerNight", ResourceType = typeof(SharedResources))]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum price cannot be negative.")]
        [Required]
        public int maxPrice{ get; set; }
    }
}
