using FCSChart.Axis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FCSChart.Series
{
    /// <summary>
    /// 散点图
    /// </summary>
    public class ScatterSeries : ISeries
    {
        #region property

        /// <summary>
        /// 散点的点的宽度
        /// </summary>
        public int PointLength
        {
            get { return (int)GetValue(PointLengthProperty); }
            set { SetValue(PointLengthProperty, value); }
        }
        public static readonly DependencyProperty PointLengthProperty = DependencyProperty.Register("PointLength", typeof(int), typeof(ScatterSeries), new PropertyMetadata(1, NeedRedrawingProperty_Changed));

        #endregion

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
            var pointLength = PointLength;
            Func<double, ValueLocationConvertParam, double> XGetLocation = xaxis.GetValueLocation;
            Func<double, ValueLocationConvertParam, double> YGetLocation = yaxis.GetValueLocation;
            var xconvert = xaxis.GetConvertParam();
            var yconvert = yaxis.GetConvertParam();
            var maxX = OwnerChart.XAxis.Max;
            var minX = OwnerChart.XAxis.Min;
            var maxY = OwnerChart.YAxis.Max;
            var minY = OwnerChart.YAxis.Min;
            using var task = Task.Run(() =>
            {
                Dictionary<int, List<int>> existed = new Dictionary<int, List<int>>();
                var streamGeometry = new StreamGeometry() { FillRule = FillRule.Nonzero };
                using (StreamGeometryContext sgc = streamGeometry.Open())
                {
                    Size size = new Size(pointLength, pointLength);
                    foreach (var item in items)
                    {
                        var valueX = Convert.ToDouble(item[x]);
                        var valueY = Convert.ToDouble(item[y]);
                        if (valueX > maxX || valueX < minX || valueY > maxY || valueY < minY) continue;
                        var xvalue = XGetLocation(valueX, xconvert);
                        var yvalue = YGetLocation(valueY, yconvert);
                        if (double.IsNaN(xvalue) || double.IsInfinity(xvalue) || double.IsNaN(yvalue) || double.IsInfinity(yvalue)) continue;
                        var xtemp = Convert.ToInt32(xvalue);
                        var ytemp = Convert.ToInt32(yvalue);
                        var point = new Point(xtemp, ytemp);
                        if (existed.ContainsKey(xtemp) && existed[xtemp].Contains(ytemp)) continue;
                        if (existed.ContainsKey(xtemp)) existed[xtemp].Add(ytemp);
                        else existed[xtemp] = new List<int>() { ytemp };
                        sgc.BeginFigure(point, true, true);
                        sgc.PolyLineTo(new Point[] { new Point(xtemp + pointLength, ytemp), new Point(xtemp + pointLength, ytemp + pointLength), new Point(xtemp, ytemp + pointLength) }, false, false);
                        //圆形点位，速度较慢
                        //sgc.ArcTo(new Point(xvalue, yvalue + pointLength), size, 0, true, SweepDirection.Clockwise, false, false);
                        //sgc.ArcTo(point, size, 0, true, SweepDirection.Clockwise, false, false);
                    }
                    sgc.Close();
                }
                streamGeometry.Freeze();
                return streamGeometry;
            });
            var geometryTemp = await task;
            using var dc = DV.RenderOpen();
            base.ClearTransform();
            dc.DrawGeometry(this.Fill, new Pen(this.Stroke, 0), geometryTemp);
            dc.Close();
        }
    }
}
