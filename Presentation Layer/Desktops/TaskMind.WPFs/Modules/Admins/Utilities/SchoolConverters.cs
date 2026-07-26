using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Utilities
{
    public class SchoolStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SchoolStatus status)
            {
                return status switch
                {
                    SchoolStatus.Pending => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    SchoolStatus.Active => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    SchoolStatus.Suspended => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                    SchoolStatus.Rejected => new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SchoolStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SchoolStatus status)
            {
                return status switch
                {
                    SchoolStatus.Pending => "Chờ duyệt",
                    SchoolStatus.Active => "Hoạt động",
                    SchoolStatus.Suspended => "Tạm ngưng",
                    SchoolStatus.Rejected => "Từ chối",
                    _ => status.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}