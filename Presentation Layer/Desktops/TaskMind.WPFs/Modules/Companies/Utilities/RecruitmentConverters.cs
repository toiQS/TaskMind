using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class JobStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is JobStatus s ? s switch
            {
                JobStatus.Draft => "Bản nháp",
                JobStatus.Open => "Đang tuyển",
                JobStatus.Closed => "Đã đóng",
                JobStatus.Filled => "Đã tuyển đủ",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class JobStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is JobStatus s ? s switch
            {
                JobStatus.Draft => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                JobStatus.Open => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                JobStatus.Closed => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                JobStatus.Filled => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class EmploymentTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is EmploymentType t ? t switch
            {
                EmploymentType.FullTime => "Toàn thời gian",
                EmploymentType.PartTime => "Bán thời gian",
                EmploymentType.Internship => "Thực tập",
                EmploymentType.Remote => "Từ xa",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class JobLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is JobLevel l ? l switch
            {
                JobLevel.Intern => "Thực tập sinh",
                JobLevel.Junior => "Junior",
                JobLevel.Mid => "Mid-level",
                JobLevel.Senior => "Senior",
                JobLevel.Lead => "Lead",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ApplicationStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ApplicationStatus s ? s switch
            {
                ApplicationStatus.New => "Mới nộp",
                ApplicationStatus.Reviewing => "Đang xem xét",
                ApplicationStatus.Interview => "Mời phỏng vấn",
                ApplicationStatus.Offered => "Đã gửi offer",
                ApplicationStatus.Rejected => "Từ chối",
                ApplicationStatus.Hired => "Đã tuyển",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ApplicationStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ApplicationStatus s ? s switch
            {
                ApplicationStatus.New => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                ApplicationStatus.Reviewing => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                ApplicationStatus.Interview => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                ApplicationStatus.Offered => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                ApplicationStatus.Rejected => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                ApplicationStatus.Hired => new SolidColorBrush(Color.FromRgb(0x2E, 0xB8, 0x5C)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Format điểm khớp kỹ năng 0-100 thành chuỗi "xx% phù hợp".</summary>
    public class MatchScoreToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is double d ? $"{d:0}% phù hợp" : "0% phù hợp";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đổi màu thanh match score: đỏ &lt; 50, vàng &lt; 75, xanh &gt;= 75.</summary>
    public class MatchScoreToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double d) return Brushes.Gray;
            if (d >= 75) return new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A));
            if (d >= 50) return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C));
            return new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp giá trị hiện tại (enum? hoặc null) với ConverterParameter — dùng cho chip lọc và ẩn/hiện nút hành động theo trạng thái.</summary>
    public class RecruitmentFilterActiveConverter : IValueConverter
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