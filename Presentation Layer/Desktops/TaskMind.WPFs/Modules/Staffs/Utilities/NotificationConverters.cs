using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
{
    public class NotificationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is NotificationType t ? t switch
            {
                NotificationType.ProjectInvite => "Mời tham gia dự án",
                NotificationType.TaskAssigned => "Công việc được giao",
                NotificationType.TestResult => "Kết quả kiểm tra",
                NotificationType.ProfileApproval => "Phê duyệt hồ sơ",
                NotificationType.Support => "Hỗ trợ",
                NotificationType.Chat => "Tin nhắn",
                NotificationType.System => "Hệ thống",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Icon WPF-UI theo từng loại thông báo — kiểm tra tên tồn tại bằng IntelliSense trước khi build.</summary>
    public class NotificationTypeToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is NotificationType t ? t switch
            {
                NotificationType.ProjectInvite => SymbolRegular.Board24,
                NotificationType.TaskAssigned => SymbolRegular.TaskListSquareLtr24,
                NotificationType.TestResult => SymbolRegular.DocumentCheckmark24,
                NotificationType.ProfileApproval => SymbolRegular.PersonAvailable24,
                NotificationType.Support => SymbolRegular.ChatHelp24,
                NotificationType.Chat => SymbolRegular.Chat24,
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
                NotificationType.ProjectInvite => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                NotificationType.TaskAssigned => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                NotificationType.TestResult => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                NotificationType.ProfileApproval => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                NotificationType.Support => new SolidColorBrush(Color.FromRgb(0xFF, 0x8A, 0x65)),
                NotificationType.Chat => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                NotificationType.System => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Nền card: sáng hơn khi CHƯA đọc để thu hút chú ý, mặc định khi đã đọc.</summary>
    public class IsReadToCardBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isRead && !isRead
                ? new SolidColorBrush(Color.FromRgb(0x1E, 0x2A, 0x3A))
                : new SolidColorBrush(Color.FromRgb(0x28, 0x2D, 0x33));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Chấm tròn báo "chưa đọc" — chỉ hiện khi IsRead = false.</summary>
    public class IsReadToDotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isRead && !isRead ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đậm tiêu đề khi chưa đọc, chữ thường khi đã đọc.</summary>
    public class IsReadToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isRead && !isRead ? FontWeights.SemiBold : FontWeights.Normal;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị badge số chưa đọc: rỗng nếu 0, "9+" nếu &gt;9, ngược lại số thật.</summary>
    public class UnreadCountToBadgeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i > 0 ? (i > 9 ? "9+" : i.ToString()) : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}