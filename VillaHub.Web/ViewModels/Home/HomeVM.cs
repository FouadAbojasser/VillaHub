using VillaHub.Domain.Entities;

namespace VillaHub.Web.ViewModels.Home
{
    public class HomeVM
    {
        public IEnumerable<Village>? Villages { get; set; }
        public int NoOfVillas {  get; set; }
        public double AvgAreaOfVillas { get; set; }
        public double AvgCapacityPerVilla { get; set; }
        public Dictionary<int, double> AvgVillaAreaPerVillage { get; set; } = [];
        public Dictionary<int, double> AvgVillaCapacityPerVillage { get; set; } = [];
    }
}
