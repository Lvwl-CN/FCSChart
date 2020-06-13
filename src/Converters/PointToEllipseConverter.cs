using FCSChart.Graphical;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FCSChart.Converters
{
    internal class PointToEllipseConverter : IValueConverter
    {
        public double RadiusX { get; set; } = 5;
        public double RadiusY { get; set; } = 5;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point p) return new EllipseGeometry(p, RadiusX, RadiusY);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EllipseGeometry geometry) return geometry.Center;
            return default(Point);
        }
    }
}
