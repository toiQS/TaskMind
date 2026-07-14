using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Utilities
{
    public class UserStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserAccountStatus status)
            {
                return status switch
                {
                    UserAccountStatus.Active => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    UserAccountStatus.Locked => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    UserAccountStatus.Banned => new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class UserStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserAccountStatus status)
            {
                return status switch
                {
                    UserAccountStatus.Active => "Hoạt động",
                    UserAccountStatus.Locked => "Tạm khoá",
                    UserAccountStatus.Banned => "Bị cấm",
                    _ => status.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class UserTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserType type)
            {
                return type switch
                {
                    UserType.Student => "Sinh viên",
                    UserType.JobSeeker => "Người tìm việc",
                    UserType.OssContributor => "OSS Contributor",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}