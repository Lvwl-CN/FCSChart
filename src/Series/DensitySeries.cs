using FCSChart.Axis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FCSChart.Series
{
    /// <summary>
    /// 密度图
    /// </summary>
    public class DensitySeries : ISeries
    {
        /// <summary>
        /// 散点的点的宽度
        /// </summary>
        public int PointLength
        {
            get { return (int)GetValue(PointLengthProperty); }
            set { SetValue(PointLengthProperty, value); }
        }
        public static readonly DependencyProperty PointLengthProperty = DependencyProperty.Register("PointLength", typeof(int), typeof(DensitySeries), new PropertyMetadata(1, NeedRedrawingProperty_Changed));
        /// <summary>
        /// 密度梯度--10的次方数
        /// </summary>
        public byte GradesBase
        {
            get { return (byte)GetValue(GradesBaseProperty); }
            set { SetValue(GradesBaseProperty, value); }
        }
        public static readonly DependencyProperty GradesBaseProperty = DependencyProperty.Register("GradesBase", typeof(byte), typeof(DensitySeries), new PropertyMetadata((byte)0x04, NeedRedrawingProperty_Changed));

        public override async void Drawing()
        {
            if (OwnerChart == null || OwnerChart.ItemsSource == null || OwnerChart.XAxis == null || OwnerChart.YAxis == null) return;
            IEnumerable<IList> items = OwnerChart.ItemsSource;
            IAxis xaxis = OwnerChart.XAxis;
            IAxis yaxis = OwnerChart.YAxis;
            if (OwnerChart.X < 0) OwnerChart.X = 0;
            int x = OwnerChart.X;
            if (OwnerChart.Y < 0) OwnerChart.Y = 1;
            int y = OwnerChart.Y;
            var geometries = await CreateGeometrys(items, xaxis, yaxis, x, y);
            using var dc = DV.RenderOpen();
            base.ClearTransform();
            for (int i = 0; i < geometries.Count; i++)
            {
                dc.DrawGeometry(new SolidColorBrush(GetGradientColor(i, geometries.Count)), new Pen(this.Stroke, 0), geometries[i]);
            }
            dc.Close();
        }


        private Task<List<StreamGeometry>> CreateGeometrys(IEnumerable<IList> items, IAxis xaxis, IAxis yaxis, int x, int y)
        {
            var areaLength = this.PointLength;
            var gradesBase = this.GradesBase;
            var width = this.ActualWidth;
            var height = this.ActualHeight;
            Func<double, ValueLocationConvertParam, double> xGetLocation = xaxis.GetValueLocation;
            Func<double, ValueLocationConvertParam, double> yGetLocation = yaxis.GetValueLocation;
            var xParam = xaxis.GetConvertParam();
            var yParam = yaxis.GetConvertParam();
            var maxX = xaxis.Max;
            var minX = xaxis.Min;
            var maxY = yaxis.Max;
            var minY = yaxis.Min;
            return Task.Run(() =>
            {
                Dictionary<int, Dictionary<int, long>> datas = new Dictionary<int, Dictionary<int, long>>();
                for (int i = 0; i < width; i += areaLength)
                {
                    datas.Add(i, new Dictionary<int, long>());
                    for (int j = 0; j < height; j += areaLength)
                    {
                        datas[i].Add(j, 0L);
                    }
                }
                foreach (var item in items)
                {
                    var valueX = Convert.ToDouble(item[x]);
                    var valueY = Convert.ToDouble(item[y]);
                    if (valueX > maxX || valueX < minX || valueY > maxY || valueY < minY) continue;
                    var xLocation = xGetLocation(valueX, xParam);
                    var yLocation = yGetLocation(valueY, yParam);
                    if (double.IsNaN(xLocation) || double.IsInfinity(xLocation) || double.IsNaN(yLocation) || double.IsInfinity(yLocation)) continue;
                    var x1 = Math.Abs(xLocation % areaLength) > 0 ? ((Convert.ToInt32(xLocation) / areaLength - 1) * areaLength) : Convert.ToInt32(xLocation);
                    var y1 = Math.Abs(yLocation % areaLength) > 0 ? ((Convert.ToInt32(yLocation) / areaLength - 1) * areaLength) : Convert.ToInt32(yLocation);
                    if (datas.ContainsKey(x1) && datas[x1].ContainsKey(y1))
                    {
                        datas[x1][y1] += 1;
                    }
                }
                var maxCount = datas.Values.Max(p => p.Values.Max(k => k));
                List<StreamGeometry> streams = new List<StreamGeometry>();
                List<StreamGeometryContext> sgcs = new List<StreamGeometryContext>();
                var count = Convert.ToInt32(Math.Log(maxCount, gradesBase));
                for (int i = 0; i <= count; i++)
                {
                    var streamGeometry = new StreamGeometry() { FillRule = FillRule.Nonzero };
                    streams.Add(streamGeometry);
                    StreamGeometryContext sgc = streamGeometry.Open();
                    sgcs.Add(sgc);
                }
                foreach (var data in datas)
                {
                    foreach (var temp in data.Value)
                    {
                        if (temp.Value <= 0L) continue;
                        var index = Convert.ToInt32(Math.Log(temp.Value, gradesBase));
                        sgcs[index].BeginFigure(new Point(data.Key, temp.Key), true, true);
                        sgcs[index].PolyLineTo(new Point[] { new Point(data.Key + areaLength, temp.Key), new Point(data.Key + areaLength, temp.Key + areaLength), new Point(data.Key, temp.Key + areaLength) }, false, false);
                    }
                }
                foreach (var sgc in sgcs) sgc.Close();
                foreach (var stream in streams) stream.Freeze();
                return streams;
            });
        }

    }
}
