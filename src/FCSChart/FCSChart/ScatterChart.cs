using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FCSChart.FCSChart
{
    /// <summary>
    /// 散点图
    /// </summary>
    public class ScatterChart : FCSChartBase
    {

        static ScatterChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScatterChart), new FrameworkPropertyMetadata(typeof(ScatterChart)));
        }

        public ScatterChart()
        {
            Series = new Series.ScatterSeries();
        }
    }
}
