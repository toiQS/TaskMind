using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Utilities
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

    /// <summary>Chuyển ProjectSource ("Company"/"School"/"OpenSource") thành nhãn tiếng Việt.</summary>
    public class ProjectSourceToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = value as string;
            return source switch
            {
                "Company" => "Dự án công ty",
                "School" => "Dự án đào tạo",
                "OpenSource" => "Mã nguồn mở",
                _ => source
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>true (đang thực hiện) -> "Đang tham gia", false -> "Đã hoàn thành".</summary>
    public class IsOngoingToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ongoing = value is bool b && b;
            return ongoing ? "Đang tham gia" : "Đã hoàn thành";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>true -> màu xanh (đang hoạt động), false -> màu xám (đã kết thúc).</summary>
    public class IsOngoingToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ongoing = value is bool b && b;
            return ongoing
                ? new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A))
                : new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}