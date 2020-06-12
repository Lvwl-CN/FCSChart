using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FCSChart.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 是否反转
        /// </summary>
        public bool IsReverse { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return (IsReverse ? !b : b) ? Visibility.Visible : Visibility.Collapsed;
            return IsReverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v) return IsReverse ? v != Visibility.Visible : v == Visibility.Visible;
            return IsReverse;
        }
    }
}
