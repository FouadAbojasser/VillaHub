namespace VillaHub.Web.ViewModels.Dashboard
{
    public class RadialBarChartVM
    {
        public double TotalCount { get; set; }
        public double CountInCurrentMonth { get; set; }
        public bool HasRatioIncreased { get; set; }
        public List<int> Series { get; set; } = [];
    }
}
