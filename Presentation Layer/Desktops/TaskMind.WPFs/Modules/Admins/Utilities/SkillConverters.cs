using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Utilities
{
    public class SkillCategoryToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SkillCategory category)
            {
                return category switch
                {
                    SkillCategory.ProgrammingLanguage => "Ngôn ngữ lập trình",
                    SkillCategory.Framework => "Framework",
                    SkillCategory.SoftSkill => "Kỹ năng mềm",
                    SkillCategory.Tool => "Công cụ",
                    SkillCategory.Other => "Khác",
                    _ => category.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SkillLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SkillLevel level)
            {
                return level switch
                {
                    SkillLevel.Beginner => "Cơ bản",
                    SkillLevel.Intermediate => "Trung bình",
                    SkillLevel.Advanced => "Thành thạo",
                    SkillLevel.Expert => "Chuyên gia",
                    _ => level.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SkillLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SkillLevel level)
            {
                return level switch
                {
                    SkillLevel.Beginner => new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA0)),
                    SkillLevel.Intermediate => new SolidColorBrush(Color.FromRgb(0x4C, 0x9A, 0xFF)),
                    SkillLevel.Advanced => new SolidColorBrush(Color.FromRgb(0x3F, 0xD0, 0x7A)),
                    SkillLevel.Expert => new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}