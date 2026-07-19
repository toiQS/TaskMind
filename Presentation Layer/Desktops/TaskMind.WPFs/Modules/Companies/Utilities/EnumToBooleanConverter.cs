using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskMind.WPFs.Utilities
{
    /// <summary>
    /// So khớp giá trị enum hiện tại với ConverterParameter (tên enum dạng string).
    /// Dùng để bind IsChecked của RadioButton menu theo mục đang được chọn (OneWay).
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}