using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Utilities
{
    /// <summary>So khớp filter đang chọn để tô đậm (Primary) nút chip đang active</summary>
    public class FilterAppearanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var current = value as string;
            var key = parameter as string;
            return string.Equals(current, key, StringComparison.OrdinalIgnoreCase)
                ? ControlAppearance.Primary
                : ControlAppearance.Secondary;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class CompanyStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CompanyStatus status)
            {
                return status switch
                {
                    CompanyStatus.Pending => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    CompanyStatus.Active => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    CompanyStatus.Suspended => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                    CompanyStatus.Rejected => new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class CompanyStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CompanyStatus status)
            {
                return status switch
                {
                    CompanyStatus.Pending => "Chờ duyệt",
                    CompanyStatus.Active => "Hoạt động",
                    CompanyStatus.Suspended => "Tạm ngưng",
                    CompanyStatus.Rejected => "Từ chối",
                    _ => status.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}