using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class ConversationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ConversationType t ? t switch
            {
                ConversationType.Direct => "Cá nhân",
                ConversationType.Project => "Dự án",
                ConversationType.Partner => "Đối tác",
                ConversationType.Admin => "Admin hệ thống",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ConversationTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ConversationType t ? t switch
            {
                ConversationType.Direct => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                ConversationType.Project => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                ConversationType.Partner => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                ConversationType.Admin => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Căn bong bóng tin nhắn: phải nếu IsMine = true, trái nếu ngược lại.</summary>
    public class IsMineToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isMine && isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Màu nền bong bóng: xanh dương cho tin của mình, xám cho tin người khác.</summary>
    public class IsMineToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isMine && isMine
                ? new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF))
                : new SolidColorBrush(Color.FromRgb(0x3A, 0x41, 0x49));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Bo góc bong bóng bất đối xứng theo IsMine (giống Zalo/Messenger).</summary>
    public class IsMineToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isMine && isMine
                ? new CornerRadius(14, 14, 2, 14)
                : new CornerRadius(14, 14, 14, 2);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class MessageStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is MessageStatus s ? s switch
            {
                MessageStatus.Sending => "Đang gửi...",
                MessageStatus.Sent => "Đã gửi",
                MessageStatus.Delivered => "Đã nhận",
                MessageStatus.Read => "Đã xem",
                _ => string.Empty
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đảo ngược bool -> Visibility (Lưu ý: BooleanToVisibilityConverter chuẩn KHÔNG hỗ trợ ConverterParameter="Invert").</summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về Visible nếu count > 0 — dùng cho badge số tin chưa đọc.</summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i > 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc hiện tại (enum? hoặc null) với ConverterParameter để tô sáng chip đang chọn.</summary>
    public class ChatFilterActiveConverter : IValueConverter
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