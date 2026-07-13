using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public class DashBroadModel
    {
        public DashbroadStatistic Statistic { get; set; } = new DashbroadStatistic();
        public List<TodoModel> TodoList { get; set; } = new List<TodoModel>();

        /// Dữ liệu cho line chart (ví dụ: tăng trưởng người dùng/doanh thu theo tháng)
        public List<ChartPoint> RevenueChart { get; set; } = new List<ChartPoint>();
    }

    public class TodoModel
    {
        public string Index { get; set; }
        public string Name { get; set; }
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
}