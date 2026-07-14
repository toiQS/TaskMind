using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Utilities
{
    public class RevenueSourceToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RevenueSourceType source)
            {
                return source switch
                {
                    RevenueSourceType.TransactionFee => "Phí giao dịch",
                    RevenueSourceType.CompanySubscription => "Phí công ty",
                    RevenueSourceType.SchoolSubscription => "Phí cơ sở đào tạo",
                    _ => source.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class RevenueSourceToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RevenueSourceType source)
            {
                return source switch
                {
                    RevenueSourceType.TransactionFee => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                    RevenueSourceType.CompanySubscription => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    RevenueSourceType.SchoolSubscription => new SolidColorBrush(Color.FromRgb(0xB8, 0x92, 0xFF)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đổi dấu % thành text kèm mũi tên tăng/giảm, dùng cho thẻ delta doanh thu.</summary>
    public class DeltaPercentToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double delta)
            {
                string arrow = delta >= 0 ? "▲" : "▼";
                return $"{arrow} {Math.Abs(delta):0.#}% so với kỳ trước";
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class DeltaPercentToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double delta)
            {
                return delta >= 0
                    ? new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A))
                    : new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D));
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}