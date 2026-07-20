using System;
using System.Globalization;
using System.Windows.Data;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    /// <summary>Hiển thị gợi ý ngắn theo ứng viên đang chọn ở chế độ "Từ ứng viên đã tuyển".</summary>
    public class HiredCandidateToSummaryTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is HiredCandidateOption c
                ? $"Kế thừa họ tên, email và kỹ năng từ hồ sơ ứng tuyển \"{c.AppliedJobTitle}\""
                : "Chọn một ứng viên đã tuyển để kế thừa thông tin";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}