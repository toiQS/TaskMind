using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskMind.WPFs.Modules.Admins.Utilities
{
    /// <summary>
    /// So sánh giá trị binding (ActiveKey) với ConverterParameter (key của từng menu item).
    /// Dùng để bind IsChecked của RadioButton menu, giúp highlight mục đang được chọn.
    /// </summary>
    public class StringEqualsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // RadioButton khi được check sẽ trả về true -> trả lại key tương ứng
            if (value is bool b && b) return parameter?.ToString();
            return Binding.DoNothing;
        }
    }
}