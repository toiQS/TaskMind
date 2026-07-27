using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Utilities
{
    public class ReportStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReportStatus status)
            {
                return status switch
                {
                    ReportStatus.Pending => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    ReportStatus.Reviewing => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                    ReportStatus.Resolved => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    ReportStatus.Dismissed => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ReportStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReportStatus status)
            {
                return status switch
                {
                    ReportStatus.Pending => "Chờ xử lý",
                    ReportStatus.Reviewing => "Đang xác minh",
                    ReportStatus.Resolved => "Đã xử lý",
                    ReportStatus.Dismissed => "Đã bỏ qua",
                    _ => status.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReportPriority priority)
            {
                return priority switch
                {
                    ReportPriority.High => new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
                    ReportPriority.Medium => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    ReportPriority.Low => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ViolationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViolationType type)
            {
                return type switch
                {
                    ViolationType.SpamContent => "Nội dung spam",
                    ViolationType.Harassment => "Quấy rối / xúc phạm",
                    ViolationType.FraudPayment => "Gian lận thanh toán",
                    ViolationType.FakeInformation => "Thông tin giả mạo",
                    ViolationType.IntellectualProperty => "Vi phạm bản quyền",
                    ViolationType.Other => "Khác",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ReportedEntityTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReportedEntityType type)
            {
                return type switch
                {
                    ReportedEntityType.User => "Người dùng",
                    ReportedEntityType.Company => "Công ty",
                    ReportedEntityType.School => "Cơ sở đào tạo",
                    ReportedEntityType.Project => "Dự án",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ResolutionActionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResolutionAction action)
            {
                return action switch
                {
                    ResolutionAction.Warning => "Cảnh cáo",
                    ResolutionAction.LockAccount => "Khoá tài khoản",
                    ResolutionAction.BanAccount => "Cấm tài khoản",
                    ResolutionAction.SuspendOrganization => "Tạm ngưng tổ chức",
                    ResolutionAction.Dismiss => "Bỏ qua, không vi phạm",
                    ResolutionAction.Other => "Khác",
                    _ => action.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}