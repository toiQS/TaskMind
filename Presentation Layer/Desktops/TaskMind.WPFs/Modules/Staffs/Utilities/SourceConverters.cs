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
                "xaml" => SymbolRegular.Code24,
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

    /// <summary>So khớp giá trị enum bất kỳ (SourceEnvironment, SourceLeftTab...) với ConverterParameter
    /// bằng ToString() — dùng cho pill tab Dev/Test/Product VÀ Duyệt/Thay đổi/Lịch sử/Lỗi.
    /// LƯU Ý QUAN TRỌNG: converter này BẮT BUỘC phải trả về Visibility (không phải bool) vì WPF không
    /// tự động chuyển đổi bool -> Visibility khi đi qua IValueConverter tường minh. Trước đây converter
    /// này trả về bool khiến binding Visibility "im lặng" thất bại và giữ nguyên giá trị mặc định
    /// (Visible) cho MỌI panel cùng lúc — đây chính là nguyên nhân gốc rễ của lỗi "view đè view" ở
    /// sidebar 4 tab (Duyệt/Thay đổi/Lịch sử/Lỗi) trong SourceView.</summary>
    public class EnvironmentEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isMatch = value != null && parameter != null
                && string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);

            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class IssueOpenToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is CodeIssueStatus s && s != CodeIssueStatus.Resolved ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

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

    // ===================== Diff / Commit / Tab (lấy cảm hứng từ GitHub Desktop) =====================

    /// <summary>Nền của 1 dòng diff: xanh nhạt cho dòng thêm, đỏ nhạt cho dòng xoá, trong suốt cho
    /// dòng không đổi.</summary>
    public class DiffLineTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DiffLineType t ? t switch
            {
                DiffLineType.Added => new SolidColorBrush(Color.FromArgb(0x33, 0x3F, 0xD0, 0x7A)),
                DiffLineType.Removed => new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x6B, 0x6B)),
                _ => Brushes.Transparent
            } : Brushes.Transparent;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Ký hiệu đầu dòng diff: "+" / "-" / khoảng trắng — đúng phong cách unified diff.</summary>
    public class DiffLineTypePrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DiffLineType t ? t switch
            {
                DiffLineType.Added => "+",
                DiffLineType.Removed => "-",
                _ => " "
            } : " ";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class DiffLineTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DiffLineType t ? t switch
            {
                DiffLineType.Added => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                DiffLineType.Removed => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                _ => new SolidColorBrush(Color.FromRgb(0x5C, 0x63, 0x70))
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class DiffChangeKindToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DiffChangeKind k ? k switch
            {
                DiffChangeKind.Added => "Mới",
                DiffChangeKind.Modified => "Sửa",
                DiffChangeKind.Deleted => "Xoá",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Icon theo loại commit — đã kiểm tra tồn tại: ArrowUndo24, DocumentEdit24, Rocket24.</summary>
    public class SourceCommitKindToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SourceCommitKind k ? k switch
            {
                SourceCommitKind.Edit => SymbolRegular.DocumentEdit24,
                SourceCommitKind.Release => SymbolRegular.Rocket24,
                SourceCommitKind.Revert => SymbolRegular.ArrowUndo24,
                _ => SymbolRegular.DocumentEdit24
            } : SymbolRegular.DocumentEdit24;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SourceCommitKindToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SourceCommitKind k ? k switch
            {
                SourceCommitKind.Edit => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                SourceCommitKind.Release => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                SourceCommitKind.Revert => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị thời gian tương đối kiểu GitHub Desktop: "vài giây trước", "5 phút trước",
    /// "2 giờ trước", "hôm qua", hoặc dd/MM/yyyy nếu đã lâu.</summary>
    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt) return string.Empty;

            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 60) return "Vài giây trước";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 2) return "Hôm qua";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày trước";
            return dt.ToString("dd/MM/yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}