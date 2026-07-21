using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Trạng thái dự án (mục 4.7 tài liệu nghiệp vụ).</summary>
    public enum ProjectStatus
    {
        InProgress, // Đang thực hiện
        Paused,     // Tạm dừng
        Completed,  // Hoàn thành
        Cancelled   // Huỷ
    }

    /// <summary>Loại dự án: nội bộ (không phí giao dịch) hoặc trao đổi với đối tác ngoài (mục 4.14).</summary>
    public enum ProjectKind
    {
        Internal,
        Exchange
    }

    /// <summary>Vai trò trong phạm vi 1 dự án (mục 3 tài liệu nghiệp vụ).</summary>
    public enum ProjectRole
    {
        Owner,
        TechnicalLeader,
        ProjectManager,
        QaQc,
        Developer,
        Intern
    }

    public class ProjectMemberModel
    {
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public ProjectRole Role { get; set; }

        /// <summary>Đánh dấu thành viên chính là nhân sự đang đăng nhập, dùng để tô sáng trong
        /// danh sách thành viên dự án.</summary>
        public bool IsMe { get; set; }
    }

    /// <summary>
    /// Dự án mà nhân sự (staff) đã hoặc đang tham gia, hiển thị trên giao diện riêng của nhân sự
    /// (module Staffs). Dữ liệu tổng thể (Name/Description/Status/Progress/Members...) đồng bộ từ
    /// ProjectModel bên Modules.Companies; các trường "My..." bên dưới là góc nhìn cá nhân của nhân
    /// sự trong phạm vi dự án này (mục 3 - vai trò dự án; mục 4.7 - "Developer thực hiện task được
    /// giao, cập nhật tiến độ công việc của bản thân").
    /// </summary>
    public class ProjectModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public string Description { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.InProgress;
        public ProjectKind Kind { get; set; } = ProjectKind.Internal;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }

        /// <summary>Tiến độ tổng thể của cả dự án, 0-100.</summary>
        public double Progress { get; set; }

        public int TaskTotal { get; set; }
        public int TaskDone { get; set; }

        public ObservableCollection<ProjectMemberModel> Members { get; set; } = new();

        /// <summary>Chỉ có giá trị khi Kind = Exchange (liên kết mục 4.14 - Quản lý trao đổi).</summary>
        public string PartnerName { get; set; }
        public decimal? ContractValue { get; set; }

        // ===== Góc nhìn cá nhân của nhân sự trong dự án này =====

        /// <summary>Vai trò của nhân sự hiện tại trong dự án (mục 3) — độc lập với vai trò ở dự án khác.</summary>
        public ProjectRole MyRole { get; set; } = ProjectRole.Developer;

        /// <summary>Ngày nhân sự được thêm vào dự án — có thể khác StartDate nếu tham gia sau.</summary>
        public DateTime MyJoinedDate { get; set; } = DateTime.Now;

        /// <summary>Số công việc được giao riêng cho nhân sự và số đã hoàn thành (mục 4.7).</summary>
        public int MyTaskTotal { get; set; }
        public int MyTaskDone { get; set; }

        public double MyProgress => MyTaskTotal > 0 ? Math.Round((double)MyTaskDone / MyTaskTotal * 100, 0) : 0;

        /// <summary>True nếu nhân sự vẫn đang tham gia (dự án chưa hoàn thành/huỷ) — dùng để phân biệt
        /// "đang tham gia" và "đã tham gia" theo đúng yêu cầu hiển thị của màn hình này.</summary>
        public bool IsOngoing => Status is ProjectStatus.InProgress or ProjectStatus.Paused;
    }
}