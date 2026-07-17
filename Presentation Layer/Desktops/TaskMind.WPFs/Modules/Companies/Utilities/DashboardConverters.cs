using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    /// <summary>Chuyển ObservableCollection&lt;ChartPointModel&gt; + kích thước vùng vẽ thành PointCollection cho Polyline.</summary>
    public class ChartPointsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return null;
            if (values[0] is not IEnumerable points) return null;
            if (values[1] is not double width || values[2] is not double height) return null;
            if (width <= 0 || height <= 0) return null;

            var list = points.Cast<ChartPointModel>().ToList();
            if (list.Count == 0) return null;

            double max = list.Max(p => p.Value);
            double min = list.Min(p => p.Value);
            double range = max - min == 0 ? 1 : max - min;
            double stepX = list.Count == 1 ? 0 : width / (list.Count - 1);

            var result = new PointCollection();
            for (int i = 0; i < list.Count; i++)
            {
                double x = i * stepX;
                double y = height - ((list[i].Value - min) / range * height);
                result.Add(new System.Windows.Point(x, y));
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ActivityPriority p ? p switch
            {
                ActivityPriority.High => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)),
                ActivityPriority.Medium => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                _ => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
            } : System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}