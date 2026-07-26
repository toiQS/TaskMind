using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Utilities
{
    public class NotificationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationType type)
            {
                return type switch
                {
                    NotificationType.System => "Hệ thống",
                    NotificationType.Approval => "Cần duyệt",
                    NotificationType.Warning => "Cảnh báo",
                    NotificationType.Success => "Thành công",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class NotificationTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationType type)
            {
                return type switch
                {
                    NotificationType.Approval => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    NotificationType.Warning => new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
                    NotificationType.Success => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    _ => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)) // System
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đảo ngược BooleanToVisibilityConverter, dùng để ẩn nút "đánh dấu đã đọc" khi thông báo đã đọc rồi.</summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về Visible khi số lượng = 0, dùng để hiện dòng "Không có thông báo nào".</summary>
    public class ZeroCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = value is int i ? i : 0;
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}