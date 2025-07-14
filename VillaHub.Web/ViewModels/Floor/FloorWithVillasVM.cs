using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.Floor
{
    public class FloorWithVillasVM
    {
        public VillaHub.Domain.Entities.Floor? Floor { get; set; }
        public VillaHub.Domain.Entities.Villa? Villa { get; set; }
        public VillaHub.Domain.Entities.Village? Village { get; set; }
        public List<Amenity> Amenities { get; set; } = [];
        public List<int> SelectedAmenityIds { get; set; } = [];

    }
}
