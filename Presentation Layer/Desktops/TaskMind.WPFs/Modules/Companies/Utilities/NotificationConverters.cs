using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class NotificationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is NotificationType t ? t switch
            {
                NotificationType.ProjectInvitation => "Mời dự án",
                NotificationType.CompanyInvitation => "Mời công ty",
                NotificationType.TestResult => "Kết quả kiểm tra",
                NotificationType.ProfileApproval => "Phê duyệt hồ sơ",
                NotificationType.Recruitment => "Tuyển dụng",
                NotificationType.Support => "Hỗ trợ nội bộ",
                NotificationType.System => "Hệ thống",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về SymbolRegular (WPF-UI) tương ứng từng loại thông báo — bind thẳng vào ui:SymbolIcon.Symbol.</summary>
    public class NotificationTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is NotificationType t ? t switch
            {
                NotificationType.ProjectInvitation => SymbolRegular.Board24,
                NotificationType.CompanyInvitation => SymbolRegular.Building24,
                NotificationType.TestResult => SymbolRegular.DocumentCheckmark24,
                NotificationType.ProfileApproval => SymbolRegular.CheckmarkCircle24,
                NotificationType.Recruitment => SymbolRegular.PersonAdd24,
                NotificationType.Support => SymbolRegular.ChatMultiple24,
                NotificationType.System => SymbolRegular.Alert24,
                _ => SymbolRegular.Alert24
            } : SymbolRegular.Alert24;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class NotificationTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is NotificationType t ? t switch
            {
                NotificationType.ProjectInvitation => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                NotificationType.CompanyInvitation => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                NotificationType.TestResult => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                NotificationType.ProfileApproval => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                NotificationType.Recruitment => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                NotificationType.Support => new SolidColorBrush(Color.FromRgb(0xFF, 0x8A, 0x65)),
                _ => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0))
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>In đậm tiêu đề nếu thông báo chưa đọc (IsRead = false).</summary>
    public class UnreadToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isRead && !isRead ? FontWeights.SemiBold : FontWeights.Normal;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiện chấm tròn báo hiệu nếu thông báo chưa đọc (IsRead = false).</summary>
    public class UnreadToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isRead && !isRead ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về Visible khi count == 0 — dùng cho trạng thái rỗng của danh sách thông báo.</summary>
    public class ZeroCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i == 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc hiện tại (enum?/bool?/null) với ConverterParameter — dùng cho các chip lọc và trạng thái highlight.</summary>
    public class NotificationFilterActiveConverter : IValueConverter
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