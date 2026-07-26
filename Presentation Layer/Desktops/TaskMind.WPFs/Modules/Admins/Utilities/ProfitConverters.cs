using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Utilities
{
    public class ProfitSourceToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProfitSource source)
            {
                return source switch
                {
                    ProfitSource.TransactionFee => "Phí giao dịch",
                    ProfitSource.MembershipFee => "Phí thành viên",
                    _ => source.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class InvoiceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InvoiceStatus status)
            {
                return status switch
                {
                    InvoiceStatus.Pending => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                    InvoiceStatus.Issued => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                    InvoiceStatus.Paid => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    InvoiceStatus.Overdue => new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class InvoiceStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InvoiceStatus status)
            {
                return status switch
                {
                    InvoiceStatus.Pending => "Chờ xuất HĐ",
                    InvoiceStatus.Issued => "Đã xuất HĐ",
                    InvoiceStatus.Paid => "Đã thanh toán",
                    InvoiceStatus.Overdue => "Quá hạn",
                    _ => status.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class CurrencyVndConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d) return d.ToString("N0", culture) + " đ";
            if (value is double db) return db.ToString("N0", culture) + " đ";
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}