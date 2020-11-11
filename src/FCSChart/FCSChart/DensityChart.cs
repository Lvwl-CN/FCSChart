using System.Windows;

namespace FCSChart.FCSChart
{
    /// <summary>
    /// 密度图
    /// </summary>
    public class DensityChart : FCSChartBase
    {
        static DensityChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DensityChart), new FrameworkPropertyMetadata(typeof(DensityChart)));
        }

        public DensityChart()
        {
            Series = new Series.DensitySeries();
        }
    }
}
