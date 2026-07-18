using System;
using System.Globalization;
using System.Windows.Data;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class CandidateSourceToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CandidateSource s ? s switch
            {
                CandidateSource.DirectApply => "Ứng tuyển trực tiếp",
                CandidateSource.Referral => "Giới thiệu nội bộ",
                CandidateSource.Headhunt => "Headhunt",
                CandidateSource.OpenSource => "Từ dự án mã nguồn mở",
                CandidateSource.Internal => "Ứng viên nội bộ",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Chuyển số sao 0-5 thành chuỗi ký tự ★/☆ để hiển thị nhanh mức đánh giá.</summary>
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int r = value is int i ? Math.Clamp(i, 0, 5) : 0;
            return new string('★', r) + new string('☆', 5 - r);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc hiện tại (enum? hoặc null) với ConverterParameter — dùng cho chip lọc
    /// và ẩn/hiện nút hành động theo trạng thái ứng viên.</summary>
    public class CandidateFilterActiveConverter : IValueConverter
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