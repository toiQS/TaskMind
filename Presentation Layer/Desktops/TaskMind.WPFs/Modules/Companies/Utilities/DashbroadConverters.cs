using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    /// <summary>Chuyển tên Symbol dạng chuỗi (lưu trong model) sang SymbolRegular để bind động icon theo dữ liệu.</summary>
    public class StringToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && Enum.TryParse<SymbolRegular>(s, out var symbol))
                return symbol;

            return SymbolRegular.Circle24;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Chuyển mã màu hex (string) trong model sang SolidColorBrush để tô accent linh hoạt theo dữ liệu.</summary>
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch { /* rơi xuống màu mặc định */ }
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Hiển thị thời gian tương đối kiểu "5 phút trước", "2 giờ trước", "Hôm qua" cho feed hoạt động.</summary>
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt) return string.Empty;

            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return "Vừa xong";
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