using System;
using System.Collections.Generic;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>
    /// Dữ liệu form "Tạo dự án mới" (mục 4.7 - Quản lý dự án trực thuộc công ty).
    /// Dùng để tập hợp input từ AddProjectVM trước khi build thành ProjectModel thật khi lưu.
    /// </summary>
    public class AddProjectModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ProjectKind Kind { get; set; } = ProjectKind.Internal;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }

        /// <summary>Chỉ có ý nghĩa khi Kind = Exchange (liên kết mục 4.14 - Quản lý trao đổi).</summary>
        public string PartnerName { get; set; }
        public decimal? ContractValue { get; set; }

        public List<ProjectMemberModel> Members { get; set; } = new();
    }

    /// <summary>
    /// Bản rút gọn của một nhân sự đang hoạt động trong công ty (nguồn: StaffVM.Staffs lọc theo
    /// StaffStatus.Active), dùng làm danh sách để chọn thành viên khi tạo dự án mới. Thành viên dự án
    /// luôn phải là nhân sự trực thuộc công ty (mục 4.5) — không cho phép nhập tay tên tuỳ ý.
    /// </summary>
    public class ProjectStaffOption
    {
        public Guid StaffId { get; set; }

        public string FullName { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public List<string> Skills { get; set; } = new();

        /// <summary>True nếu nhân sự này đã được thêm vào danh sách thành viên của dự án đang tạo.</summary>
        public bool IsAdded { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
        public string SkillsDisplay => Skills is { Count: > 0 } ? string.Join(" · ", Skills) : "Chưa cập nhật kỹ năng";
    }
}