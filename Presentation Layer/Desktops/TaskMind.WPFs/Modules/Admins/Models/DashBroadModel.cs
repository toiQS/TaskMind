using System;
using System.Collections.Generic;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public class DashBroadModel
    {
        public DashbroadStatistic Statistic { get; set; } = new DashbroadStatistic();
        public List<TodoModel> TodoList { get; set; } = new List<TodoModel>();

        /// <summary>
        /// Dữ liệu cho line chart (ví dụ: tăng trưởng người dùng/doanh thu theo tháng)
        /// </summary>
        public List<ChartPoint> RevenueChart { get; set; } = new List<ChartPoint>();

        /// <summary>Danh sách thông báo hệ thống gửi tới Admin (duyệt công ty, cảnh báo vi phạm, v.v.)</summary>
        public List<NotificationModel> NotificationList { get; set; } = new List<NotificationModel>();
    }

    public class TodoModel
    {
        public string Index { get; set; }
        public string Name { get; set; }

        /// <summary>1 = Cao, 2 = Trung bình, 3 = Thấp</summary>
        public int PriorityLevel { get; set; }
    }

    /// <summary>
    /// Một điểm dữ liệu trên line chart (ví dụ: tháng - giá trị)
    /// </summary>
    public class ChartPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }

    public class DashbroadStatistic
    {
        public int CountAllUsers { get; set; }
        public int CountNewUsers { get; set; }
        public int CountAllProject { get; set; }
        public int CountNewProjects { get; set; }
        public int CountAllCompanies { get; set; }
        public int CountNewCompanies { get; set; }
        public int CountAllSchools { get; set; }
        public int CountNewSchools { get; set; }
        public int CountAllTeachers { get; set; }
        public int CountNewTeacher { get; set; }
        public int CountAllStaff { get; set; }
        public int CountNewStaff { get; set; }
    }

    /// <summary>Phân loại thông báo hệ thống, dùng để tô màu/nhóm trên UI.</summary>
    public enum NotificationType
    {
        System,     // Thông báo hệ thống chung
        Approval,   // Cần duyệt (công ty / cơ sở đào tạo / kỹ năng đề xuất...)
        Warning,    // Cảnh báo, vi phạm
        Success     // Xác nhận thao tác thành công
    }

    public class NotificationModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; } = NotificationType.System;
        public DateTime CreatedDate { get; set; }

        /// <summary>true = đã đọc, false = chưa đọc</summary>
        public bool IsRead { get; set; }
    }
}