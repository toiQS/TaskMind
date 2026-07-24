using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Staffs.Models;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Modules.Staffs.Utilities
{
    public class SupportStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportStatus s ? s switch
            {
                SupportStatus.Pending => "Chờ xử lý",
                SupportStatus.InProgress => "Đang xử lý",
                SupportStatus.Resolved => "Đã giải quyết",
                SupportStatus.Closed => "Đã đóng",
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
                SupportStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                SupportStatus.Resolved => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                SupportStatus.Closed => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SupportCategoryToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportCategory c ? c switch
            {
                SupportCategory.Account => "Tài khoản",
                SupportCategory.Technical => "Kỹ thuật",
                SupportCategory.Salary => "Lương thưởng",
                SupportCategory.Project => "Dự án",
                SupportCategory.Other => "Khác",
                _ => value.ToString()
            } : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Icon WPF-UI theo từng nhóm yêu cầu — kiểm tra lại tên enum SymbolRegular bằng IntelliSense
    /// trước khi build (Person24, Wrench24, Money24, Board24, QuestionCircle24).</summary>
    public class SupportCategoryToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SupportCategory c ? c switch
            {
                SupportCategory.Account => SymbolRegular.Person24,
                SupportCategory.Technical => SymbolRegular.Wrench24,
                SupportCategory.Salary => SymbolRegular.Money24,
                SupportCategory.Project => SymbolRegular.Board24,
                SupportCategory.Other => SymbolRegular.QuestionCircle24,
                _ => SymbolRegular.QuestionCircle24
            } : SymbolRegular.QuestionCircle24;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>So khớp bộ lọc trạng thái hiện tại (enum? hoặc null) với ConverterParameter — dùng cho chip lọc.</summary>
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