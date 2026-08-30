using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class JobPostingListItemDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public JobPostingStatus PostingStatus { get; set; }
        public int RequiredSkillCount { get; set; }
        public int ApplicationCount { get; set; }
    }

    public class JobPostingDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public JobPostingStatus PostingStatus { get; set; }
        public List<RequiredSkillDto> RequiredSkills { get; set; } = new();
        public int ApplicationCount { get; set; }
        public List<JobApplicationListItemDto> RecentApplications { get; set; } = new();
    }

    public class RequiredSkillDto
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
    }

    /// <summary>Bộ lọc tin tuyển dụng cho Admin giám sát toàn nền tảng (mục 4.18).</summary>
    public class GetJobPostingsFilter
    {
        public Guid? CompanyId { get; set; }
        public JobPostingStatus? PostingStatus { get; set; }
        public string? Keyword { get; set; } // tìm theo Title/CompanyName
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class JobApplicationListItemDto
    {
        public Guid Id { get; set; }
        public Guid JobPostingId { get; set; }
        public string JobPostingTitle { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTime AppliedAtUtc { get; set; }
    }

    public class JobApplicationDetailDto : JobApplicationListItemDto
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }

    public class GetJobApplicationsFilter
    {
        public Guid? JobPostingId { get; set; }
        public Guid? UserId { get; set; }
        public ApplicationStatus? ApplicationStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Thống kê tổng quan tuyển dụng toàn nền tảng, có thể ghép vào DashboardOverviewDto sau này (mục 4.18 + 4.14).</summary>
    public class RecruitmentStatsDto
    {
        public int TotalJobPostings { get; set; }
        public int OpenJobPostings { get; set; }
        public int TotalApplications { get; set; }
        public List<JobPostingStatusCountDto> PostingsByStatus { get; set; } = new();
        public List<ApplicationStatusCountDto> ApplicationsByStatus { get; set; } = new();
        public List<TopCompanyByPostingDto> TopCompaniesByPostingCount { get; set; } = new();
    }

    public class JobPostingStatusCountDto
    {
        public JobPostingStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class ApplicationStatusCountDto
    {
        public ApplicationStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class TopCompanyByPostingDto
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int PostingCount { get; set; }
    }

    public class DashboardRecruitmentStatsDto
    {
        public int TotalJobPostings { get; set; }
        public int TotalJobsClosers { get; set; }
        public List<NodeChatDto> JobPostingsByStatus { get; set; } = new();
    }

    public class NodeChatDto
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
    }

}