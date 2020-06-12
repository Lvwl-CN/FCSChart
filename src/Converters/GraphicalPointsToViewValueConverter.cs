using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FCSChart.Converters
{
    /// <summary>
    /// axis坐标值点位集合转化为图形显示位置值集合-用于多边形门绘制
    /// </summary>
    public class GraphicalPointsToViewValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Point> points && parameter is Chart chart)
            {
                PointCollection resultPoints = new PointCollection();
                foreach (var point in points)
                {
                    var xvalue = chart.XAxis.GetValueLocation(point.X);
                    var yvalue = chart.YAxis.GetValueLocation(point.Y);
                    resultPoints.Add(new Point(xvalue, yvalue));
                }
                return resultPoints;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PointCollection points && parameter is Chart chart)
            {
                ObservableCollection<Point> resultPoints = new ObservableCollection<Point>();
                foreach (var point in points)
                {
                    var xvalue = chart.XAxis.GetLocationValue(point.X);
                    var yvalue = chart.YAxis.GetLocationValue(point.Y);
                    resultPoints.Add(new Point(xvalue, yvalue));
                }
                return resultPoints;
            }
            return null;
        }
    }
}
