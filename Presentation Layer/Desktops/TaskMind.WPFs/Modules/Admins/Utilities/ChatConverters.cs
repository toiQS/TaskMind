using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Utilities
{
    public class ChatPartnerTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChatPartnerType type)
            {
                return type switch
                {
                    ChatPartnerType.Company => "Công ty",
                    ChatPartnerType.School => "Cơ sở đào tạo",
                    ChatPartnerType.User => "Người dùng",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>true (Admin) -> Right, false (đối phương) -> Left</summary>
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool isMine && isMine) ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}