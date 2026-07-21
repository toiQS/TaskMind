using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;

namespace TaskMind.WPFs.Modules.Companies.Utilities
{
    /// <summary>Màu badge vai trò dự án (mục 3) — dùng cho chip thành viên trong AddProjectView.
    /// (ProjectConverters.cs hiện chỉ có RoleToTextConverter, thiếu bản đổi màu.)</summary>
    public class RoleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ProjectRole r ? r switch
            {
                ProjectRole.Owner => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x4C)),
                ProjectRole.TechnicalLeader => new SolidColorBrush(Color.FromRgb(0x9A, 0x7B, 0xFF)),
                ProjectRole.ProjectManager => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                ProjectRole.QaQc => new SolidColorBrush(Color.FromRgb(0xFF, 0x8A, 0x65)),
                ProjectRole.Developer => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                ProjectRole.Intern => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                _ => Brushes.Gray
            } : Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}