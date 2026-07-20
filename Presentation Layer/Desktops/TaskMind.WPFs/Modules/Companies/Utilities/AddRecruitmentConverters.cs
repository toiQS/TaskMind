using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    /// <summary>Hiển thị số ngày còn lại tới hạn nộp hồ sơ dạng "Còn X ngày" cho ô chọn hạn tuyển dụng.
    /// Trả về chuỗi rỗng nếu chưa chọn hạn, tránh phải dùng B2V (vốn chỉ nhận bool) trên DateTime?.</summary>
    public class DeadlineToRemainingTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt) return string.Empty;

            var days = (dt.Date - DateTime.Now.Date).Days;
            if (days < 0) return "Đã quá hạn";
            if (days == 0) return "Hạn nộp hôm nay";
            return $"Còn {days} ngày";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}