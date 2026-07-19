using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class TrendDirectionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TrendDirection d ? d switch
            {
                TrendDirection.Up => "▲",
                TrendDirection.Down => "▼",
                _ => "■"
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class TrendDirectionToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TrendDirection d ? d switch
            {
                TrendDirection.Up => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                TrendDirection.Down => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0))
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class AvailabilityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is FreelancerAvailability a ? a switch
            {
                FreelancerAvailability.Available => "Sẵn sàng",
                FreelancerAvailability.Busy => "Đang bận",
                FreelancerAvailability.Unavailable => "Không nhận việc",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class AvailabilityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is FreelancerAvailability a ? a switch
            {
                FreelancerAvailability.Available => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                FreelancerAvailability.Busy => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                FreelancerAvailability.Unavailable => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Format điểm khớp kỹ năng 0-100 thành chuỗi "xx% khớp".</summary>
    public class FindMatchScoreToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is double d ? $"{d:0}% khớp" : "0% khớp";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đổi màu thanh match score: đỏ &lt; 40, vàng &lt; 70, xanh &gt;= 70.</summary>
    public class FindMatchScoreToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double d) return Brushes.Gray;
            if (d >= 70) return new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A));
            if (d >= 40) return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C));
            return new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp giá trị hiện tại (enum?/object) với ConverterParameter — dùng cho chip lọc và highlight lựa chọn.</summary>
    public class FindFilterActiveConverter : IValueConverter
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