using LChart.Axis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LChart.Series
{
    /// <summary>
    /// 密度图--基于位图
    /// </summary>
    public class DensityBitmapSeries : ISeries
    {
        private Color background = Colors.White;

        public Color Background
        {
            get { return background; }
            set
            {
                background = value;
                if (value != null) BackgroundColor = new byte[] { value.A, value.R, value.G, value.B };
                else BackgroundColor = new byte[] { 0xff, 0xff, 0xff, 0xff };
            }
        }

        public byte[] BackgroundColor { get; private set; } = { 0xff, 0xff, 0xff, 0xff };

        public override async void Drawing()
        {
            if (OwnerChart == null || OwnerChart.ItemsSource == null || OwnerChart.XAxis == null || OwnerChart.YAxis == null) return;
            IEnumerable<IList> items = OwnerChart.ItemsSource;
            IAxis xaxis = OwnerChart.XAxis;
            IAxis yaxis = OwnerChart.YAxis;
            int x = OwnerChart.X;
            int y = OwnerChart.Y;

            Func<double, ValueLocationConvertParam, double> xGetLocation = xaxis.GetLocation;
            Func<double, ValueLocationConvertParam, double> yGetLocation = yaxis.GetLocation;
            int xIntLength = Convert.ToInt32(this.ActualWidth);
            int yIntLength = Convert.ToInt32(this.ActualHeight);
            var xConvertParam = xaxis.GetConvertParam();
            var yConvertParam = yaxis.GetConvertParam();
            Func<long, long, Color> getGradientColor = this.GetGradientColor;

            using var task = Task.Run(() =>
            {
                long maxCount = 1;
                Dictionary<int, Dictionary<int, long>> points = new Dictionary<int, Dictionary<int, long>>();
                foreach (var item in items)
                {
                    var xvalue = xGetLocation(Convert.ToDouble(item[x]), xConvertParam);
                    var yvalue = yGetLocation(Convert.ToDouble(item[y]), yConvertParam);
                    if (double.IsNaN(xvalue) || double.IsInfinity(xvalue) || double.IsNaN(yvalue) || double.IsInfinity(yvalue)) continue;
                    var xv = Convert.ToInt32(xvalue);
                    var yv = Convert.ToInt32(yvalue);
                    if (xv >= xIntLength || xv < 0 || yv >= yIntLength || yv < 0) continue;
                    if (points.ContainsKey(xv))
                    {
                        var temp = points[xv];
                        if (temp.ContainsKey(yv)) temp[yv] += 1;
                        else temp[yv] = 1;
                    }
                    else
                    {
                        points[xv] = new Dictionary<int, long>
                        {
                            [yv] = 1
                        };
                    }
                }
                maxCount = points.Max(p => p.Value.Values.Max());

                var bitmap = new WriteableBitmap(xIntLength, yIntLength, 96, 96, PixelFormats.Bgr32, null);
                #region 背景色
                byte[] backcolor = new byte[xIntLength * 4];
                for (int i = 0; i < xIntLength; i++) BackgroundColor.CopyTo(backcolor, i * 4);
                for (int i = 0; i < yIntLength; i++)
                {
                    Int32Rect rect = new Int32Rect(0, i, xIntLength, 1);
                    bitmap.WritePixels(rect, backcolor, backcolor.Length, 0);
                }
                #endregion

                try
                {
                    bitmap.Lock();
                    foreach (var line in points)
                    {
                        foreach (var point in line.Value)
                        {
                            unsafe
                            {
                                // Get a pointer to the back buffer.
                                IntPtr pBackBuffer = bitmap.BackBuffer;
                                // Find the address of the pixel to draw.
                                pBackBuffer += point.Key * bitmap.BackBufferStride;
                                pBackBuffer += line.Key * 4;
                                // Assign the color data to the pixel.
                                var color = getGradientColor(point.Value, maxCount);
                                *(int*)pBackBuffer = (color.R << 16) | (color.G << 8) | (color.B);
                            }
                            bitmap.AddDirtyRect(new Int32Rect(line.Key, point.Key, 1, 1));
                        }
                    }
                }
                finally
                {
                    bitmap.Unlock();
                }
                bitmap.Freeze();
                return bitmap;
            });
            var bitmap = await task;
            using var dc = DV.RenderOpen();
            base.ClearTransform();
            dc.DrawImage(bitmap, new Rect(new Size(xIntLength, yIntLength)));
            dc.Close();
        }


        public override void Move(double x, double y)
        {
            Drawing();
        }
    }
}
