using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
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

    /// <summary>Màu badge vai trò dự án (mục 3), dùng cho MyRole và vai trò từng thành viên.</summary>
    public class RoleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectRole r ? r switch
            {
                ProjectRole.Owner => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                ProjectRole.TechnicalLeader => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                ProjectRole.ProjectManager => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                ProjectRole.QaQc => new SolidColorBrush(Color.FromRgb(0xFF, 0x8A, 0x65)),
                ProjectRole.Developer => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                ProjectRole.Intern => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class KindToExchangeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectKind k && k == ProjectKind.Exchange
                ? Visibility.Visible
                : Visibility.Collapsed;
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

    /// <summary>Đổi màu thanh tiến độ công việc cá nhân: đỏ &lt; 40, vàng &lt; 75, xanh &gt;= 75.</summary>
    public class TaskProgressToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double d) return Brushes.Gray;
            if (d >= 75) return new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A));
            if (d >= 40) return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C));
            return new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc trạng thái hiện tại (enum? hoặc null) với ConverterParameter — dùng cho chip lọc.</summary>
    public class ProjectFilterActiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null) return true;
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}