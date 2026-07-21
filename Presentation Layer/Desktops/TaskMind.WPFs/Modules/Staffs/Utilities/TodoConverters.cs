using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
{
    public class TodoStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoStatus s ? s switch
            {
                TodoStatus.NotStarted => "Chưa bắt đầu",
                TodoStatus.InProgress => "Đang thực hiện",
                TodoStatus.Completed => "Hoàn thành",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class TodoStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoStatus s ? s switch
            {
                TodoStatus.NotStarted => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                TodoStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                TodoStatus.Completed => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class TodoPriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoPriority p ? p switch
            {
                TodoPriority.Low => "Thấp",
                TodoPriority.Medium => "Trung bình",
                TodoPriority.High => "Cao",
                TodoPriority.Urgent => "Khẩn cấp",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class TodoPriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoPriority p ? p switch
            {
                TodoPriority.Low => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                TodoPriority.Medium => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                TodoPriority.High => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                TodoPriority.Urgent => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đổi màu chữ hạn chót: đỏ nếu quá hạn (IsOverdue), trắng nếu bình thường. Bind từ TodoItemModel.IsOverdue.</summary>
    public class OverdueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool overdue && overdue
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B))
                : new SolidColorBrush(Color.FromRgb(0xC7, 0xCD, 0xD6));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Gạch ngang tiêu đề công việc đã hoàn thành.</summary>
    public class CompletedToTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoStatus s && s == TodoStatus.Completed ? TextDecorations.Strikethrough : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Mờ đi các card đã hoàn thành để nổi bật việc còn dang dở.</summary>
    public class CompletedToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoStatus s && s == TodoStatus.Completed ? 0.6 : 1.0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Ẩn nút "đánh dấu hoàn thành nhanh" khi công việc đã hoàn thành rồi.</summary>
    public class NotCompletedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TodoStatus s && s == TodoStatus.Completed ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc hiện tại (enum? hoặc null) với ConverterParameter — dùng cho chip lọc và ẩn/hiện nút hành động.</summary>
    public class TodoFilterActiveConverter : IValueConverter
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

    /// <summary>Trả về Visible khi count == 0 — dùng cho trạng thái rỗng của danh sách/bình luận.</summary>
    public class TodoZeroCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i == 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đảo ngược bool -> Visibility (BooleanToVisibilityConverter chuẩn KHÔNG hỗ trợ ConverterParameter="Invert").
    /// Dùng để ẩn phần "xem" khi đang IsEditing/IsCreating và ngược lại.</summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}