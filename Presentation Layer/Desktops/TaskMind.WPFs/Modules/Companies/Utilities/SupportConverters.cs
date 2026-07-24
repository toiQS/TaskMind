using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class SupportStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportStatus s ? s switch
            {
                SupportStatus.Pending => "Chờ duyệt",
                SupportStatus.Approved => "Đã duyệt",
                SupportStatus.InProgress => "Đang xử lý",
                SupportStatus.Completed => "Hoàn tất",
                SupportStatus.Rejected => "Từ chối",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SupportStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportStatus s ? s switch
            {
                SupportStatus.Pending => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                SupportStatus.Approved => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                SupportStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                SupportStatus.Completed => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                SupportStatus.Rejected => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SupportTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportType t ? t switch
            {
                SupportType.DeviceUpgrade => "Nâng cấp thiết bị",
                SupportType.Hiring => "Tuyển thêm nhân sự",
                SupportType.EnvironmentUpdate => "Cập nhật môi trường",
                SupportType.LicensePurchase => "Mua bản quyền",
                SupportType.Other => "Khác",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SupportPriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportPriority p ? p switch
            {
                SupportPriority.Low => "Thấp",
                SupportPriority.Medium => "Trung bình",
                SupportPriority.High => "Cao",
                SupportPriority.Urgent => "Khẩn cấp",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SupportPriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportPriority p ? p switch
            {
                SupportPriority.Low => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                SupportPriority.Medium => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                SupportPriority.High => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                SupportPriority.Urgent => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc hiện tại (enum? hoặc null) với ConverterParameter để tô sáng chip đang chọn.</summary>
    public class SupportFilterActiveConverter : IValueConverter
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