using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FCSChart.Converters
{
    /// <summary>
    /// axis坐标值点位转化为图形显示位置值
    /// </summary>
    public class GraphicalPointToViewValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point point && parameter is Chart chart)
            {
                var xvalue = chart.XAxis.GetValueLocation(point.X);
                var yvalue = chart.YAxis.GetValueLocation(point.Y);
                return new Point(xvalue, yvalue);

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point point && parameter is Chart chart)
            {
                var xvalue = chart.XAxis.GetLocationValue(point.X);
                var yvalue = chart.YAxis.GetLocationValue(point.Y);
                return new Point(xvalue, yvalue);
            }
            return null;
        }
    }
}
