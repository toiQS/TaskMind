using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
{
    public class StaffStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is StaffStatus s ? s switch
            {
                StaffStatus.Active => "Đang làm việc",
                StaffStatus.Suspended => "Tạm ngưng",
                StaffStatus.Resigned => "Đã nghỉ việc",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class StaffStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is StaffStatus s ? s switch
            {
                StaffStatus.Active => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                StaffStatus.Suspended => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                StaffStatus.Resigned => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SkillLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SkillLevel l ? l switch
            {
                SkillLevel.Basic => "Cơ bản",
                SkillLevel.Intermediate => "Trung bình",
                SkillLevel.Proficient => "Thành thạo",
                SkillLevel.Expert => "Chuyên gia",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SkillLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SkillLevel l ? l switch
            {
                SkillLevel.Basic => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                SkillLevel.Intermediate => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                SkillLevel.Proficient => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                SkillLevel.Expert => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Quy đổi mức độ thành thạo sang 0-100 để hiển thị thanh ProgressBar bên cạnh mỗi kỹ năng.</summary>
    public class SkillLevelToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SkillLevel l ? l switch
            {
                SkillLevel.Basic => 25.0,
                SkillLevel.Intermediate => 50.0,
                SkillLevel.Proficient => 75.0,
                SkillLevel.Expert => 100.0,
                _ => 0.0
            } : 0.0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ProfileVisibilityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ProfileVisibility v ? v switch
            {
                ProfileVisibility.Public => "Công khai",
                ProfileVisibility.CompanyOnly => "Chỉ công ty",
                ProfileVisibility.Private => "Riêng tư",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SocialPlatformToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SocialPlatform p ? p switch
            {
                SocialPlatform.GitHub => "GitHub",
                SocialPlatform.GitLab => "GitLab",
                SocialPlatform.LinkedIn => "LinkedIn",
                SocialPlatform.Website => "Website cá nhân",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị "x xác nhận" hoặc "Chưa có xác nhận" cho mỗi kỹ năng (mục 4.3 - endorsement).</summary>
    public class EndorsementCountToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i > 0 ? $"{i} xác nhận" : "Chưa có xác nhận";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đảo ngược bool -> Visibility.</summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về Visible nếu count > 0 — dùng khi danh sách có dữ liệu (học vấn/kinh nghiệm/dự án).</summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i > 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về Visible nếu count == 0 — dùng cho trạng thái rỗng của danh sách.</summary>
    public class ZeroCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i == 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Nhãn trạng thái cho một dòng lịch sử dự án: "Hoàn thành" / "Đang tham gia".</summary>
    public class ProjectCompletedToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? "Hoàn thành" : "Đang tham gia";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Màu badge trạng thái dòng lịch sử dự án: xanh lá khi hoàn thành, xanh dương khi đang tham gia.</summary>
    public class ProjectCompletedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A))
                : new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}