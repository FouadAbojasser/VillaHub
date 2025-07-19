namespace VillaHub.Web.ViewModels.Dashboard
{
    public class LineChartVM
    {
        public List<ChartData> Series { get; set; } = [];
        public string[] Categories { get; set; } = null!;
    }

    public class ChartData
    {
        public string Name { get; set; } = string.Empty;
        public int[] Data { get; set; } = null!;
    }
}
