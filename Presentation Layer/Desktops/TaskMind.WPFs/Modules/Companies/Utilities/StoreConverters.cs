using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class ListingTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ListingType t
                ? (t == ListingType.OpenSource ? "Mã nguồn mở" : "Dự án trao đổi")
                : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ListingStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ListingStatus s ? s switch
            {
                ListingStatus.PendingApproval => "Chờ duyệt",
                ListingStatus.Published => "Đang hiển thị",
                ListingStatus.Negotiating => "Đang thương lượng",
                ListingStatus.Sold => "Đã trao đổi",
                ListingStatus.Rejected => "Bị từ chối",
                ListingStatus.Closed => "Đã gỡ",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ListingStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ListingStatus s ? s switch
            {
                ListingStatus.PendingApproval => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                ListingStatus.Published => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                ListingStatus.Negotiating => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                ListingStatus.Sold => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                ListingStatus.Rejected => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                ListingStatus.Closed => new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x7B)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị giá: "Miễn phí / Trao đổi" cho OpenSource hoặc Price null, ngược lại format tiền tệ.</summary>
    public class PriceDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is decimal p && p > 0 ? $"{p:N0} đ" : "Miễn phí / Trao đổi";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp giá trị hiện tại (enum? hoặc null) với ConverterParameter để tô sáng chip đang chọn / ẩn hiện theo trạng thái.</summary>
    public class StoreFilterActiveConverter : IValueConverter
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