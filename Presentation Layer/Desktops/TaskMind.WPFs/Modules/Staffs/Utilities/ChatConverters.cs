using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
{
    public class ConversationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ConversationType t ? t switch
            {
                ConversationType.Direct => "Cá nhân",
                ConversationType.Group => "Nhóm",
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
                ConversationType.Group => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về SymbolRegular (WPF-UI) tương ứng loại hội thoại — bind thẳng vào ui:SymbolIcon.Symbol.</summary>
    public class ConversationTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ConversationType t && t == ConversationType.Group
                ? SymbolRegular.PeopleTeam24
                : SymbolRegular.Person24;

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

    /// <summary>Trả về SymbolRegular (WPF-UI) tương ứng trạng thái tin nhắn — bind thẳng vào ui:SymbolIcon.Symbol.</summary>
    public class MessageStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is MessageStatus s ? s switch
            {
                MessageStatus.Sending => SymbolRegular.Circle24,
                MessageStatus.Sent => SymbolRegular.Checkmark24,
                MessageStatus.Delivered => SymbolRegular.CheckmarkCircle24,
                MessageStatus.Read => SymbolRegular.CheckmarkCircle24,
                _ => SymbolRegular.Circle24
            } : SymbolRegular.Circle24;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị giờ tin nhắn/hội thoại: "HH:mm" nếu trong hôm nay, "Hôm qua" nếu hôm qua,
    /// ngược lại "dd/MM". Trả về rỗng nếu chưa có tin nhắn nào (LastMessageTime = DateTime.MinValue).</summary>
    public class ChatTimeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt || dt == DateTime.MinValue) return string.Empty;

            var today = DateTime.Now.Date;
            if (dt.Date == today) return dt.ToString("HH:mm");
            if (dt.Date == today.AddDays(-1)) return "Hôm qua";
            return dt.ToString("dd/MM");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Trả về Visible nếu đang trực tuyến (IsOnline = true) — dùng cho chấm báo online cạnh avatar.</summary>
    public class OnlineToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}