using System;
using System.Globalization;
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

    /// <summary>Đổi màu đỏ khi công việc quá hạn (IsOverdue = true), ngược lại giữ màu chữ phụ mặc định.</summary>
    public class OverdueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B))
                : new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị hạn hoàn thành dạng "Còn X ngày" / "Quá hạn X ngày" / "Hạn hôm nay".</summary>
    public class DueDateToRemainingTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt) return "Không có hạn";

            var days = (dt.Date - DateTime.Now.Date).Days;
            if (days == 0) return "Hạn hôm nay";
            if (days > 0) return $"Còn {days} ngày";
            return $"Quá hạn {Math.Abs(days)} ngày";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}