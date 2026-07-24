namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Trạng thái nhân sự trong công ty.</summary>
    public enum StaffStatus
    {
        Active,     // Đang hoạt động
        Suspended,  // Tạm ngưng
        Resigned    // Đã rời công ty
    }

    /// <summary>
    /// Nhân sự trực thuộc công ty (mục 4.5). Khi tuyển dụng thành công (mục 5.1 - HireCommand),
    /// một phần dữ liệu (họ tên, email, kỹ năng) được kế thừa từ hồ sơ công khai của ứng viên
    /// thông qua SourceCandidateId, phần còn lại (chức danh, phòng ban, ngày gia nhập, hợp đồng)
    /// được bổ sung thủ công khi thêm nhân sự.
    /// </summary>
    public class StaffModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string Position { get; set; }
        public string Department { get; set; }

        public StaffStatus Status { get; set; } = StaffStatus.Active;

        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime? LeftDate { get; set; }

        public List<string> Skills { get; set; } = new();
        public List<string> ProjectNames { get; set; } = new();

        public string SourceCandidateName { get; set; }
        public string Note { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
        public string SkillsDisplay => Skills is { Count: > 0 } ? string.Join(" · ", Skills) : "Chưa cập nhật kỹ năng";
        public string ProjectsDisplay => ProjectNames is { Count: > 0 } ? string.Join(" · ", ProjectNames) : "Chưa phân bổ dự án";
        public int ProjectCount => ProjectNames?.Count ?? 0;

        /// <summary>Chỉ hiện nút "Cho nghỉ việc" khi chưa Resigned.</summary>
        public bool CanResign => Status != StaffStatus.Resigned;
    }
}