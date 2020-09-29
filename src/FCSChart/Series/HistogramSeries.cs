using FCSChart.Axis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FCSChart.Series
{
    /// <summary>
    /// 直方图
    /// </summary>
    public class HistogramSeries : ISeries
    {

        /// <summary>
        /// 直方图等份数量
        /// </summary>
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(HistogramSeries), new PropertyMetadata(512, NeedRedrawingProperty_Changed));

        internal override async void Drawing()
        {
            if (OwnerChart == null || OwnerChart.ItemsSource == null || OwnerChart.XAxis == null || OwnerChart.YAxis == null) return;
            IEnumerable<IList> items = OwnerChart.ItemsSource;
            IAxis xaxis = OwnerChart.XAxis;
            IAxis yaxis = OwnerChart.YAxis;
            if (OwnerChart.X < 0) OwnerChart.X = 0;
            int x = OwnerChart.X;
            if (OwnerChart.Y < 0) OwnerChart.Y = 1;
            int y = OwnerChart.Y;
            using var getdatatask = xaxis.GetHistogramViewData(items, x, Count);
            var datas = await getdatatask;
            if (datas.Count() <= 0)
            {
                var tempdc = DV.RenderOpen();
                tempdc.DrawLine(new Pen(this.Stroke, 0), new Point(0, 0), new Point(0, 0));
                tempdc.Close();
                return;
            }
            var maxy = datas.Max(p => p.Value);
            yaxis.Max = maxy * 1.2;
            yaxis.Min = 0;
            yaxis.Drawing();
            var height = this.ActualHeight;
            Func<double, ValueLocationConvertParam, double> YGetLocation = yaxis.GetValueLocation;
            var yconvert = yaxis.GetConvertParam();
            var task = Task.Run(() =>
            {
                var streamGeometry = new StreamGeometry() { FillRule = FillRule.EvenOdd };
                using (StreamGeometryContext sgc = streamGeometry.Open())
                {
                    List<Point> points = new List<Point>();
                    foreach (var data in datas)
                    {
                        var yvalue = YGetLocation(data.Value, yconvert);
                        if (double.IsNaN(data.Key[0]) || double.IsInfinity(data.Key[0])
                        || double.IsNaN(data.Key[1]) || double.IsInfinity(data.Key[1])
                        || double.IsNaN(yvalue) || double.IsInfinity(yvalue)) continue;
                        points.Add(new Point(data.Key[0], yvalue));
                        points.Add(new Point(data.Key[1], yvalue));
                    }
                    if (points.Count > 0)
                    {
                        points.Add(new Point(points[points.Count - 1].X, height));
                        points.Add(new Point(points[0].X, height));
                        sgc.BeginFigure(new Point(points[0].X, height), true, true);
                        sgc.PolyLineTo(points, false, false);
                    }
                    else
                    {
                        sgc.BeginFigure(new Point(0, 0), true, true);
                        sgc.LineTo(new Point(0, 0), false, false);
                    }
                    sgc.Close();
                }
                streamGeometry.Freeze();
                return streamGeometry;
            });
            var geometryTemp = await task;
            task.Dispose();
            var dc = DV.RenderOpen();
            base.ClearTransform();
            dc.DrawGeometry(this.Fill, new Pen(this.Stroke, 1), geometryTemp);
            dc.Close();
        }
    }
}
