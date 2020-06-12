using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FCSChart.FCSChart
{
    /// <summary>
    /// 登高线图
    /// </summary>
    public class ContourChart : FCSChartBase
    {
        static ContourChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContourChart), new FrameworkPropertyMetadata(typeof(ContourChart)));
        }

        public ContourChart()
        {
            Series = new Series.ContourSeries();
        }
    }
}
