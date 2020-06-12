using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FCSChart.Converters
{
    internal class AxisLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double d) && parameter is Axis.AxisType a)
            {
                switch (a)
                {
                    case Axis.AxisType.X:
                        return new Thickness(d, 0, 0, 0);
                    case Axis.AxisType.Y:
                        return new Thickness(0, 0, 0, d);
                    default:
                        return value;
                }
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
