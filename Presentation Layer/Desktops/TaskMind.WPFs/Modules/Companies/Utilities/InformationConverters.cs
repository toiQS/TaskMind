using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    public class VerificationStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CompanyVerificationStatus s ? s switch
            {
                CompanyVerificationStatus.PendingVerification => "Chờ xác thực",
                CompanyVerificationStatus.Verified => "Đã xác thực",
                CompanyVerificationStatus.Rejected => "Bị từ chối",
                CompanyVerificationStatus.Suspended => "Đã tạm khoá",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class VerificationStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CompanyVerificationStatus s ? s switch
            {
                CompanyVerificationStatus.PendingVerification => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                CompanyVerificationStatus.Verified => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                CompanyVerificationStatus.Rejected => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                CompanyVerificationStatus.Suspended => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đổi màu số ngày còn lại của gói: đỏ khi sắp hết hạn (&lt;=7 ngày), xanh khi còn nhiều.</summary>
    public class ExpiringSoonToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool soon && soon
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B))
                : new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}