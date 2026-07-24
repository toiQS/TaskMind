using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    public enum JobStatus { Draft, Open, Closed, Filled }
    public enum EmploymentType { FullTime, PartTime, Internship, Remote }
    public enum JobLevel { Intern, Junior, Mid, Senior, Lead }
    public enum ApplicationStatus { New, Reviewing, Interview, Offered, Rejected, Hired }

    /// <summary>Tin tuyển dụng gắn yêu cầu kỹ năng (tham chiếu danh mục kỹ năng, mục 5.1 + 4.15).</summary>
    public class JobPostingModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }

        public JobLevel Level { get; set; } = JobLevel.Junior;
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
        public JobStatus Status { get; set; } = JobStatus.Draft;
        public bool IsSelected { get; set; }
        public System.Collections.Generic.List<string> RequiredSkills { get; set; } = new();

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool SalaryNegotiable { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;
        public DateTime? Deadline { get; set; }

        public int ViewCount { get; set; }

        public ObservableCollection<CandidateApplicationModel> Applications { get; set; } = new();

        public int ApplicationsCount => Applications.Count;
        public int NewApplicationsCount => Applications.Count(a => a.Status == ApplicationStatus.New);

        public string SkillsDisplay => RequiredSkills is { Count: > 0 } ? string.Join(" · ", RequiredSkills) : "Chưa gắn kỹ năng";

        public string SalaryDisplay
        {
            get
            {
                if (SalaryMin is null && SalaryMax is null) return SalaryNegotiable ? "Thoả thuận" : "Chưa công bố";
                var min = SalaryMin.HasValue ? $"{SalaryMin:N0}" : "?";
                var max = SalaryMax.HasValue ? $"{SalaryMax:N0}" : "?";
                return $"{min} - {max} đ" + (SalaryNegotiable ? " (có thể thoả thuận)" : "");
            }
        }
    }

    /// <summary>
    /// Ứng viên nộp đơn: ứng tuyển bằng hồ sơ cá nhân + lịch sử dự án/kỹ năng sẵn có (mục 5.1).
    /// MatchScore mô phỏng gợi ý ứng viên phù hợp dựa trên mức độ khớp kỹ năng.
    /// </summary>
    public class CandidateApplicationModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
        public DateTime AppliedDate { get; set; } = DateTime.Now;

        /// <summary>Mức độ khớp kỹ năng, 0-100 (tính từ SkillProfile ứng viên so với RequiredSkills).</summary>
        public double MatchScore { get; set; }

        public System.Collections.Generic.List<string> MatchedSkills { get; set; } = new();

        public string ResumeUrl { get; set; }
        public string Note { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
        public string MatchedSkillsDisplay => MatchedSkills is { Count: > 0 } ? string.Join(" · ", MatchedSkills) : "—";
    }
}