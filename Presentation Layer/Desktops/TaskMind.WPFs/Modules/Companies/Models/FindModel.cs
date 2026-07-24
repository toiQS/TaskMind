namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Xu hướng tăng/giảm nhu cầu tuyển dụng theo từng kỹ năng.</summary>
    public enum TrendDirection { Up, Down, Stable }

    /// <summary>Trạng thái sẵn sàng nhận việc của ứng viên tự do.</summary>
    public enum FreelancerAvailability { Available, Busy, Unavailable }

    /// <summary>Thẻ đang xem trên màn "Tìm kiếm": ứng viên tự do hay khách hàng tiềm năng (công ty).</summary>
    public enum FindScope
    {
        Candidate,  // Tìm ứng viên tự do tham gia dự án
        Company     // Tìm khách hàng tiềm năng (công ty cần thuê ngoài/hợp tác dự án)
    }

    /// <summary>Trạng thái theo dõi quan hệ với một công ty tiềm năng (pipeline khai thác khách hàng).</summary>
    public enum CompanyLeadStatus
    {
        New,            // Mới được hệ thống gợi ý, chưa liên hệ
        Contacted,      // Đã liên hệ
        InTalks,        // Đang trao đổi/thương lượng
        Converted,      // Đã trở thành đối tác/khách hàng chính thức
        NotInterested   // Từ chối/không quan tâm
    }

    /// <summary>Thống kê xu hướng nhu cầu kỹ năng hiện tại trên toàn hệ thống (mục 5.6).</summary>
    public class SkillTrendModel
    {
        public string SkillName { get; set; }
        public int DemandCount { get; set; }
        public double ChangePercent { get; set; }
        public TrendDirection Direction { get; set; } = TrendDirection.Stable;
    }

    /// <summary>Dự án của công ty đang cần tìm thêm ứng viên tự do tham gia.</summary>
    public class ProjectNeedModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public int OpenSlots { get; set; }
        public bool IsSelected { get; set; }
        public string SkillsDisplay => RequiredSkills is { Count: > 0 } ? string.Join(" · ", RequiredSkills) : "Chưa xác định";
    }

    /// <summary>
    /// Ứng viên tự do (freelancer/người tìm việc/OSS contributor) có thể tìm kiếm và mời tham gia dự án (mục 5.6).
    /// MatchScore tính theo mức khớp kỹ năng cá nhân với dự án đang chọn.
    /// </summary>
    public class FreelanceCandidateModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Headline { get; set; }
        public List<string> Skills { get; set; } = new();

        public int ExperienceYears { get; set; }
        public double Rating { get; set; }
        public int CompletedProjects { get; set; }
        public FreelancerAvailability Availability { get; set; } = FreelancerAvailability.Available;
        public string Location { get; set; }
        public string PortfolioUrl { get; set; }

        /// <summary>Tính lại mỗi khi đổi dự án đang chọn (xem RecalculateMatchScores trong VM).</summary>
        public double MatchScore { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
        public string SkillsDisplay => Skills is { Count: > 0 } ? string.Join(" · ", Skills) : "Chưa cập nhật kỹ năng";
    }

    /// <summary>
    /// Công ty tiềm năng (khách hàng tiềm năng) — công ty khác đang có nhu cầu thuê ngoài/hợp tác dự án
    /// (liên kết mục 4.14 - Quản lý trao đổi), do hệ thống gợi ý dựa trên nhu cầu công nghệ khớp với
    /// năng lực hiện có của công ty mình. Dùng cho thẻ "Khách hàng tiềm năng" trên màn Tìm kiếm (mục 5.6).
    /// </summary>
    public class PotentialCompanyModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string CompanySize { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }

        /// <summary>Công nghệ/kỹ năng công ty này đang cần cho các dự án sắp/đang triển khai.</summary>
        public List<string> NeededSkills { get; set; } = new();

        /// <summary>Số dự án đang cần tìm đối tác/outsource.</summary>
        public int OpenProjectsCount { get; set; }

        /// <summary>Ngân sách ước tính cho hợp tác — có thể để trống nếu chưa xác định (thoả thuận).</summary>
        public decimal? EstimatedBudget { get; set; }

        public CompanyLeadStatus Status { get; set; } = CompanyLeadStatus.New;

        /// <summary>Mức độ phù hợp giữa nhu cầu công nghệ của họ và năng lực hiện có của công ty mình, 0-100.</summary>
        public double MatchScore { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }

        /// <summary>Ghi chú nội bộ về quá trình tiếp cận (cuộc gọi, email đã gửi,...).</summary>
        public string Note { get; set; }

        /// <summary>Dùng để tô sáng card đang được chọn trong danh sách (không phải INotifyPropertyChanged).</summary>
        public bool IsSelected { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(CompanyName) ? "?" : CompanyName.Trim()[0].ToString().ToUpper();

        public string SkillsDisplay => NeededSkills is { Count: > 0 } ? string.Join(" · ", NeededSkills) : "Chưa cập nhật nhu cầu";

        public string BudgetDisplay => EstimatedBudget.HasValue ? $"{EstimatedBudget:N0} đ" : "Chưa xác định / thoả thuận";
    }
}