using FCSChart.Axis;
using System.Windows;
using System.Windows.Data;

namespace FCSChart.FCSChart
{
    /// <summary>
    /// 直方图
    /// </summary>
    public class HistogramChart : FCSChartBase
    {
        static HistogramChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HistogramChart), new FrameworkPropertyMetadata(typeof(HistogramChart)));
        }

        public HistogramChart()
        {
            Series = new Series.HistogramSeries();
        }

        protected override void FillYAxisList()
        {
            var line = new LinearNumberAxis();
            Binding maxbinding = new Binding("YMax") { Mode = BindingMode.TwoWay };
            Binding minbinding = new Binding("YMin") { Mode = BindingMode.TwoWay };
            minbinding.Source = maxbinding.Source = this;
            line.SetBinding(IAxis.MaxProperty, maxbinding);
            line.SetBinding(IAxis.MinProperty, minbinding);
            //YAxisList = new List<IAxis>() { line };
            YAxis = line;
        }
    }
}
