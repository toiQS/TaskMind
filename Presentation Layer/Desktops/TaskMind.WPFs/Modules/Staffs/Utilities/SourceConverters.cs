using System;
using System.Globalization;
using System.Windows;
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
                SourceEnvironment.Dev => "Development",
                SourceEnvironment.Test => "Testing",
                SourceEnvironment.Product => "Production",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SourceEnvironmentToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SourceEnvironment e ? e switch
            {
                SourceEnvironment.Dev => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                SourceEnvironment.Test => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                SourceEnvironment.Product => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Nhãn nút release: "Release lên Testing" / "Release lên Production" — rỗng ở Production.</summary>
    public class EnvironmentToReleaseLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SourceEnvironment e ? e switch
            {
                SourceEnvironment.Dev => "Release lên Testing",
                SourceEnvironment.Test => "Release lên Production",
                _ => string.Empty
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Icon thư mục/file (WPF-UI SymbolRegular) theo loại nút + phần mở rộng — bind thẳng
    /// vào ui:SymbolIcon.Symbol.</summary>
    public class SourceNodeToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SourceNodeModel node) return SymbolRegular.Document24;

            if (node.IsFolder)
                return node.IsExpanded ? SymbolRegular.FolderOpen24 : SymbolRegular.Folder24;

            return node.Extension switch
            {
                "cs" => SymbolRegular.Code24,
                "xaml" => SymbolRegular.Layout24,
                "json" => SymbolRegular.Braces24,
                "csproj" or "sln" => SymbolRegular.Apps24,
                _ => SymbolRegular.Document24
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueSeverityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CodeIssueSeverity s ? s switch
            {
                CodeIssueSeverity.Low => "Thấp",
                CodeIssueSeverity.Medium => "Trung bình",
                CodeIssueSeverity.High => "Cao",
                CodeIssueSeverity.Critical => "Nghiêm trọng",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueSeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CodeIssueSeverity s ? s switch
            {
                CodeIssueSeverity.Low => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                CodeIssueSeverity.Medium => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                CodeIssueSeverity.High => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                CodeIssueSeverity.Critical => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CodeIssueStatus s ? s switch
            {
                CodeIssueStatus.Open => "Chưa xử lý",
                CodeIssueStatus.InProgress => "Đang sửa",
                CodeIssueStatus.Resolved => "Đã sửa",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CodeIssueStatus s ? s switch
            {
                CodeIssueStatus.Open => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                CodeIssueStatus.InProgress => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                CodeIssueStatus.Resolved => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp môi trường hiện tại với ConverterParameter — dùng cho pill tab Dev/Test/Product.</summary>
    public class EnvironmentEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null && parameter != null && string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Ẩn nút "Đánh dấu đã sửa" khi thông báo lỗi đã ở trạng thái Resolved.</summary>
    public class IssueOpenToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CodeIssueStatus s && s != CodeIssueStatus.Resolved ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Chỉ hiện nút Release khi (1) nhân sự có quyền release ở dự án này VÀ (2) môi trường hiện
    /// tại không phải Production (Production là điểm cuối, không release đi đâu tiếp nữa).</summary>
    public class CanReleaseVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Collapsed;

            var canRelease = values[0] is bool b && b;
            var notProduct = values[1] is SourceEnvironment e && e != SourceEnvironment.Product;

            return canRelease && notProduct ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}