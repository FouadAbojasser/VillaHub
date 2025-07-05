using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.Villa
{
    public class VillaWithVillagesVM
    {
        public VillaHub.Domain.Entities.Villa? Villa { get; set; }
        public IEnumerable<SelectListItem> Villages { get; set; } = [];
                      
    }
}
