using System;
using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Companies.Models
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

    /// <summary>
    /// Một thành viên trong phạm vi 1 dự án. StaffId luôn tham chiếu tới một nhân sự có thật
    /// trực thuộc công ty (StaffModel, mục 4.5) — thành viên dự án không được nhập tay tự do,
    /// phải chọn từ danh sách nhân sự đang hoạt động khi khởi tạo/quản lý dự án (mục 4.7).
    /// </summary>
    public class ProjectMemberModel
    {
        public Guid StaffId { get; set; }

        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public ProjectRole Role { get; set; }

        /// <summary>Dùng cho avatar tròn — tránh lỗi Name[0] binding trực tiếp trong XAML.</summary>
        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
    }

    public class ProjectModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public string Description { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.InProgress;
        public ProjectKind Kind { get; set; } = ProjectKind.Internal;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }

        /// <summary>Tiến độ tổng thể, 0-100.</summary>
        public double Progress { get; set; }

        public int TaskTotal { get; set; }
        public int TaskDone { get; set; }

        public ObservableCollection<ProjectMemberModel> Members { get; set; } = new();

        /// <summary>Chỉ có giá trị khi Kind = Exchange (liên kết mục 4.14 - Quản lý trao đổi).</summary>
        public string PartnerName { get; set; }
        public decimal? ContractValue { get; set; }
    }
}