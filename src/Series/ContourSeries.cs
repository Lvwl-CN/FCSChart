using FCSChart.Axis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FCSChart.Series
{
    /// <summary>
    /// 等高线图
    /// </summary>
    public class ContourSeries : ISeries
    {
        /// <summary>
        /// 散点的点的宽度
        /// </summary>
        public int PointLength
        {
            get { return (int)GetValue(PointLengthProperty); }
            set { SetValue(PointLengthProperty, value); }
        }
        public static readonly DependencyProperty PointLengthProperty = DependencyProperty.Register("PointLength", typeof(int), typeof(ContourSeries), new PropertyMetadata(1, NeedRedrawingProperty_Changed));

        /// <summary>
        /// 小区域的长宽大小
        /// </summary>
        public byte AreaLength
        {
            get { return (byte)GetValue(AreaLengthProperty); }
            set { SetValue(AreaLengthProperty, value); }
        }
        public static readonly DependencyProperty AreaLengthProperty = DependencyProperty.Register("AreaLength", typeof(byte), typeof(ContourSeries), new PropertyMetadata((byte)0x0a, NeedRedrawingProperty_Changed));

        /// <summary>
        /// 渐变基数--log的底,每增加一个次方，就增加一条等高线
        /// </summary>
        public byte GradesBase
        {
            get { return (byte)GetValue(GradesBaseProperty); }
            set { SetValue(GradesBaseProperty, value); }
        }
        public static readonly DependencyProperty GradesBaseProperty = DependencyProperty.Register("GradesBase", typeof(byte), typeof(ContourSeries), new PropertyMetadata((byte)0x06, NeedRedrawingProperty_Changed));

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
                dc.DrawGeometry(null, new Pen(new SolidColorBrush(GetGradientColor(i, geometries.Count)), 1), geometries[i]);
            }
            dc.Close();
        }

        private Task<List<StreamGeometry>> CreateGeometrys(IEnumerable<IList> items, IAxis xaxis, IAxis yaxis, int x, int y)
        {
            var pointLength = this.PointLength;
            var areaLength = this.AreaLength;
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
                Dictionary<int, Dictionary<int, Area>> measureData = new Dictionary<int, Dictionary<int, Area>>();
                for (int i = 0; i < width; i += areaLength)
                {
                    measureData.Add(i, new Dictionary<int, Area>());
                    for (int j = 0; j < height; j += areaLength)
                    {
                        measureData[i].Add(j, new Area(i, j, i + areaLength, j + areaLength));
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
                    if (measureData.ContainsKey(x1) && measureData[x1].ContainsKey(y1))
                    {
                        var temp = measureData[x1][y1];
                        temp.Points.Add(new Point(xLocation, yLocation));
                    }
                }
                var maxCount = measureData.Values.Max(p => p.Values.Max(k => k.PointsCount));
                List<StreamGeometry> streams = new List<StreamGeometry>();
                List<StreamGeometryContext> sgcs = new List<StreamGeometryContext>();
                var count = Math.Log(maxCount, gradesBase);// Convert.ToInt32(maxCount / subrange);
                for (int i = 0; i <= count; i++)
                {
                    var streamGeometry = new StreamGeometry() { FillRule = FillRule.Nonzero };
                    streams.Add(streamGeometry);
                    StreamGeometryContext sgc = streamGeometry.Open();
                    sgcs.Add(sgc);
                }
                foreach (var data in measureData)
                {
                    foreach (var temp in data.Value)
                    {
                        DrawingVisual(sgcs, GetNearArea(measureData, temp.Value), gradesBase, temp.Value, pointLength, areaLength);
                    }
                }
                foreach (var sgc in sgcs) sgc.Close();
                foreach (var stream in streams) stream.Freeze();
                return streams;
            });
        }

        /// <summary>
        /// 获取附近区域的点数
        /// </summary>
        /// <param name="items"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private long[] GetNearArea(Dictionary<int, Dictionary<int, Area>> items, Area target)
        {
            if (items == null || items.Count <= 0) return null;
            long[] areas = new long[4];
            areas[0] = target.PointsCount;
            if (items.ContainsKey(target.IntX2) && items[target.IntX2].ContainsKey(target.IntY1))
                areas[1] = items[target.IntX2][target.IntY1].PointsCount;
            if (items.ContainsKey(target.IntX2) && items[target.IntX2].ContainsKey(target.IntY2))
                areas[2] = items[target.IntX2][target.IntY2].PointsCount;
            if (items.ContainsKey(target.IntX1) && items[target.IntX1].ContainsKey(target.IntY2))
                areas[3] = items[target.IntX1][target.IntY2].PointsCount;
            return areas;
        }

        /// <summary>
        /// 绘制图形-Marching Squares算法
        /// </summary>
        /// <param name="sgcs"></param>
        /// <param name="datas"></param>
        /// <param name="gradesBase"></param>
        /// <param name="area"></param>
        /// <param name="pointLength"></param>
        /// <param name="areaLength"></param>
        private void DrawingVisual(List<StreamGeometryContext> sgcs, long[] datas, double gradesBase, Area area, int pointLength, int areaLength)
        {
            if (datas[0] < gradesBase && datas[1] < gradesBase && datas[2] < gradesBase && datas[3] < gradesBase)
            {
                DrawingPoint(sgcs[0], area.Points, pointLength);
                return;
            }
            double nextX = area.CenterX + areaLength;
            double nextY = area.CenterY + areaLength;
            for (int i = 1; i <= sgcs.Count; i++)
            {
                var range = Math.Pow(gradesBase, i);
                var num = ((datas[0] >= range ? 1 : 0) << 3)
                    + ((datas[1] >= range ? 1 : 0) << 2)
                    + ((datas[2] >= range ? 1 : 0) << 1)
                    + (datas[3] >= range ? 1 : 0);
                if (num == 0) break;
                switch (num)
                {
                    case 1:
                        DrawingLine(sgcs[i], new Point(area.CenterX, area.Y2), new Point(area.X2, nextY));
                        break;
                    case 2:
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.X2, nextY));
                        break;
                    case 3:
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.CenterX, area.Y2));
                        break;
                    case 4:
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.X2, area.CenterY));
                        break;
                    case 5:
                        DrawingLine(sgcs[i], new Point(area.CenterX, area.Y2), new Point(area.X2, area.CenterY));
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.X2, nextY));
                        break;
                    case 6:
                        DrawingLine(sgcs[i], new Point(area.X2, nextY), new Point(area.X2, area.CenterY));
                        break;
                    case 7:
                        DrawingLine(sgcs[i], new Point(area.CenterX, area.Y2), new Point(area.X2, area.CenterY));
                        break;
                    case 8:
                        DrawingLine(sgcs[i], new Point(area.CenterX, area.Y2), new Point(area.X2, area.CenterY));
                        break;
                    case 9:
                        DrawingLine(sgcs[i], new Point(area.X2, nextY), new Point(area.X2, area.CenterY));
                        break;
                    case 10:
                        DrawingLine(sgcs[i], new Point(area.CenterX, area.Y2), new Point(area.X2, nextY));
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.X2, area.CenterY));
                        break;
                    case 11:
                        DrawingLine(sgcs[i], new Point(area.X2, area.CenterY), new Point(nextX, area.Y2));
                        break;
                    case 12:
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.CenterX, area.Y2));
                        break;
                    case 13:
                        DrawingLine(sgcs[i], new Point(nextX, area.Y2), new Point(area.X2, nextY));
                        break;
                    case 14:
                        DrawingLine(sgcs[i], new Point(area.X2, nextY), new Point(area.CenterX, area.Y2));
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 没有梯度的区域里的点绘制出来
        /// </summary>
        /// <param name="sgc"></param>
        /// <param name="points"></param>
        /// <param name="pointLength"></param>
        private void DrawingPoint(StreamGeometryContext sgc, IEnumerable<Point> points, int pointLength)
        {
            foreach (var point in points)
            {
                sgc.BeginFigure(new Point(point.X, point.Y), true, true);
                sgc.PolyLineTo(new Point[] { new Point(point.X + pointLength, point.Y), new Point(point.X + pointLength, point.Y + pointLength), new Point(point.X, point.Y + pointLength) }, false, false);
            }
        }

        private void DrawingLine(StreamGeometryContext sgc, Point start, Point lineto)
        {
            if (start == null || lineto == null) return;
            sgc.BeginFigure(start, true, false);
            sgc.LineTo(lineto, true, false);
        }

    }

    public struct Area
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public List<Point> Points { get; set; }
        public int PointsCount
        {
            get
            {
                if (Points == null) return 0;
                return Points.Count;
            }
        }
        public Area(double x1, double y1, double x2, double y2)
        {
            this.X1 = x1;
            this.Y1 = y1;
            this.X2 = x2;
            this.Y2 = y2;
            Points = new List<Point>();
            IntX1 = Convert.ToInt32(x1);
            IntY1 = Convert.ToInt32(y1);
            IntX2 = Convert.ToInt32(x2);
            IntY2 = Convert.ToInt32(y2);
            CenterX = (x1 + x2) / 2;
            CenterY = (y1 + y2) / 2;
        }

        public int IntX1 { get; private set; }
        public int IntY1 { get; private set; }
        public int IntX2 { get; private set; }
        public int IntY2 { get; private set; }

        public double CenterX { get; private set; }
        public double CenterY { get; private set; }
    }
}
