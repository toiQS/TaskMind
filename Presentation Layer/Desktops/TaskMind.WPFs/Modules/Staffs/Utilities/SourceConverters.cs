using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
{
    public class SourceEnvironmentToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SourceEnvironment e ? e switch
            {
                SourceEnvironment.Development => "Môi trường Dev",
                SourceEnvironment.Testing => "Môi trường Test",
                SourceEnvironment.Production => "Môi trường Production",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueSeverityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is IssueSeverity s ? s switch
            {
                IssueSeverity.Low => "Thấp",
                IssueSeverity.Medium => "Trung bình",
                IssueSeverity.High => "Cao",
                IssueSeverity.Critical => "Nghiêm trọng",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueSeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is IssueSeverity s ? s switch
            {
                IssueSeverity.Low => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                IssueSeverity.Medium => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                IssueSeverity.High => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                IssueSeverity.Critical => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is IssueStatus s ? s switch
            {
                IssueStatus.Open => "Chưa xử lý",
                IssueStatus.InProgress => "Đang xử lý",
                IssueStatus.Resolved => "Đã sửa",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is IssueStatus s ? s switch
            {
                IssueStatus.Open => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                IssueStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                IssueStatus.Resolved => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Icon WPF-UI theo loại nút trong cây kiến trúc dự án — kiểm tra lại tên enum SymbolRegular
    /// bằng IntelliSense trước khi build (Folder24, Document24).</summary>
    public class IsFolderToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isFolder && isFolder ? SymbolRegular.Folder24 : SymbolRegular.Document24;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Đảo ngược bool -> bool (khác InverseBooleanToVisibilityConverter ở ProfileConverters.cs,
    /// dùng khi cần bind thẳng vào 1 thuộc tính bool như IsReadOnly, không phải Visibility).</summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}