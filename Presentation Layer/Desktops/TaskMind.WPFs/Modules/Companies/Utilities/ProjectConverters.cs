using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectStatus s ? s switch
            {
                ProjectStatus.InProgress => "Đang thực hiện",
                ProjectStatus.Paused => "Tạm dừng",
                ProjectStatus.Completed => "Hoàn thành",
                ProjectStatus.Cancelled => "Đã huỷ",
                _ => value.ToString()
            } : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectStatus s ? s switch
            {
                ProjectStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                ProjectStatus.Paused => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                ProjectStatus.Completed => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                ProjectStatus.Cancelled => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => Brushes.Gray
            } : Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class KindToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectKind k
                ? (k == ProjectKind.Exchange ? "Dự án trao đổi" : "Dự án nội bộ")
                : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class RoleToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectRole r ? r switch
            {
                ProjectRole.Owner => "Chủ dự án",
                ProjectRole.TechnicalLeader => "Trưởng nhóm kỹ thuật",
                ProjectRole.ProjectManager => "Quản lý dự án",
                ProjectRole.QaQc => "QA/QC",
                ProjectRole.Developer => "Lập trình viên",
                ProjectRole.Intern => "Thực tập sinh",
                _ => value.ToString()
            } : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class KindToExchangeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectKind k && k == ProjectKind.Exchange
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Chuyển 0-100 thành chuỗi "xx%".</summary>
    public class ProgressToPercentTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double d ? $"{d:0}%" : "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}