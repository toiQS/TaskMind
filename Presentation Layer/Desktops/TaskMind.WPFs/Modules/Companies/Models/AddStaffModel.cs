namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Nguồn dữ liệu khi thêm nhân sự mới (mục 4.5): kế thừa từ ứng viên đã tuyển
    /// (mục 5.1 - HireCommand) hoặc nhập thủ công hoàn toàn.</summary>
    public enum AddStaffSourceMode
    {
        FromCandidate,
        Manual
    }

    /// <summary>
    /// Bản rút gọn của một ứng viên đã ở trạng thái Hired (ApplicationStatus.Hired), lấy từ
    /// RecruitmentVM/CandidateVM, dùng để chọn làm nguồn kế thừa họ tên/email/kỹ năng khi thêm nhân sự.
    /// </summary>
    public class HiredCandidateOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Email { get; set; }
        public string AppliedJobTitle { get; set; }
        public List<string> Skills { get; set; } = new();

        /// <summary>Dùng để tô sáng card đang được chọn trong danh sách (không phải INotifyPropertyChanged).</summary>
        public bool IsSelected { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
        public string SkillsDisplay => Skills is { Count: > 0 } ? string.Join(" · ", Skills) : "Chưa cập nhật kỹ năng";
    }
}