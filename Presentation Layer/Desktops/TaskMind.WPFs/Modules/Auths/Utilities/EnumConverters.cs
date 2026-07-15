using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TaskMind.WPFs.Utilities
{
    /// <summary>
    /// So khớp enum hiện tại với ConverterParameter (tên enum dạng string) để quyết định Visibility.
    /// Dùng chung cho mọi enum (ForgotPasswordStep, v.v...).
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}