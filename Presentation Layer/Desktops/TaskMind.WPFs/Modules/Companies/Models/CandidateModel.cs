namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Nguồn ứng viên.</summary>
    public enum CandidateSource
    {
        DirectApply,   // Ứng tuyển trực tiếp qua tin tuyển dụng
        Referral,      // Được giới thiệu nội bộ
        Headhunt,      // Do bộ phận tuyển dụng chủ động tìm
        OpenSource,    // Từ đóng góp dự án mã nguồn mở trên hệ thống
        Internal       // Ứng viên nội bộ (chuyển vị trí)
    }

    /// <summary>
    /// Ứng viên tổng hợp trên toàn công ty (không giới hạn theo 1 tin tuyển dụng cụ thể),
    /// dùng cho màn "Danh sách ứng viên" (mục 5.1 - Quản lý tuyển dụng và ứng tuyển).
    /// Gộp hồ sơ cá nhân + lịch sử dự án/kỹ năng sẵn có trên hệ thống + file CV đính kèm.
    /// Tái sử dụng ApplicationStatus đã khai báo tại RecruitmentModel.cs để đồng bộ trạng thái
    /// giữa "Ứng viên theo tin tuyển dụng" và "Danh sách ứng viên" tổng.
    /// </summary>
    public class CandidateModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }

        /// <summary>Vị trí đang ứng tuyển (liên kết JobPostingModel ở màn Recruitment).</summary>
        public string AppliedJobTitle { get; set; }
        public Guid? JobPostingId { get; set; }

        public CandidateSource Source { get; set; } = CandidateSource.DirectApply;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
        public DateTime AppliedDate { get; set; } = DateTime.Now;

        /// <summary>Mức độ khớp kỹ năng do hệ thống gợi ý, 0-100 (mục 5.1 - matching).</summary>
        public double MatchScore { get; set; }

        public List<string> Skills { get; set; } = new();
        public List<string> MatchedSkills { get; set; } = new();

        /// <summary>Lịch sử dự án đã tham gia trên hệ thống, tự động tổng hợp (mục 4.2).</summary>
        public List<string> ProjectHistory { get; set; } = new();

        public string CvFileName { get; set; }
        public string CvUrl { get; set; }
        public string PortfolioUrl { get; set; }
        public string CoverLetter { get; set; }

        /// <summary>Ghi chú nội bộ của HR/Admin company, không hiển thị cho ứng viên.</summary>
        public string InternalNote { get; set; }

        /// <summary>Đánh giá nhanh của nhà tuyển dụng, 0-5 sao.</summary>
        public int Rating { get; set; }

        /// <summary>Lịch sử phản hồi trao đổi với ứng viên (liên kết mục 5.3 - Notification).</summary>
        public List<CandidateResponseModel> Responses { get; set; } = new();

        /// <summary>Dùng để tô sáng card ứng viên đang được chọn trong danh sách (không phải INotifyPropertyChanged).</summary>
        public bool IsSelected { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();

        public string SkillsDisplay => Skills is { Count: > 0 } ? string.Join(" · ", Skills) : "Chưa cập nhật kỹ năng";
        public string MatchedSkillsDisplay => MatchedSkills is { Count: > 0 } ? string.Join(" · ", MatchedSkills) : "—";

        public bool HasCv => !string.IsNullOrWhiteSpace(CvUrl);
        public bool HasPortfolio => !string.IsNullOrWhiteSpace(PortfolioUrl);
        public int ResponseCount => Responses?.Count ?? 0;
    }

    /// <summary>Một lượt phản hồi/trao đổi giữa HR và ứng viên xung quanh hồ sơ ứng tuyển.</summary>
    public class CandidateResponseModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
        public DateTime SentDate { get; set; } = DateTime.Now;

        /// <summary>True nếu do HR/công ty gửi, false nếu là phản hồi/ghi chú từ phía ứng viên.</summary>
        public bool IsFromCompany { get; set; } = true;
        public string SenderName { get; set; }
    }
}