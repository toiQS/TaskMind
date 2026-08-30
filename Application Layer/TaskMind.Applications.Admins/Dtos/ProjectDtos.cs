using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class ProjectListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ProjectSourceType SourceType { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
        public bool IsExchangeProject { get; set; }
        public Guid? OwningEntityId { get; set; }
        public string? OwningEntityName { get; set; }
        public int ActiveMemberCount { get; set; }
    }

    public class ProjectDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectSourceType SourceType { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
        public bool IsExchangeProject { get; set; }
        public Guid? OwningEntityId { get; set; }
        public string? OwningEntityName { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public List<ProjectMemberDetailDto> Members { get; set; } = new();
        public bool HasActiveExchangeContract { get; set; }
    }

    public class ProjectMemberDetailDto
    {
        public Guid AccountId { get; set; }
        public ProjectRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }

        /// <summary>Loại tài khoản của AccountId — vì ProjectMember.AccountId tham chiếu đa hình (mục 8: vấn đề mở).</summary>
        public string AccountType { get; set; } = string.Empty; // "User" | "Staff" | "Student" | "Teacher"
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>Bộ lọc dự án cho Admin giám sát toàn nền tảng.</summary>
    public class GetProjectsFilter
    {
        public ProjectSourceType? SourceType { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public bool? IsExchangeProject { get; set; }
        public Guid? OwningEntityId { get; set; }
        public string? Keyword { get; set; } // tìm theo Title
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Thống kê tổng quan dự án toàn nền tảng (bổ sung chi tiết hơn ProjectsByStatus trong Dashboard).</summary>
    public class ProjectStatsDto
    {
        public int TotalProjects { get; set; }
        public List<ProjectStatusCountDto> ByStatus { get; set; } = new();
        public List<ProjectSourceTypeCountDto> BySourceType { get; set; } = new();
        public int ExchangeProjectCount { get; set; }
        public int InternalProjectCount { get; set; }
    }

    public class ProjectSourceTypeCountDto
    {
        public ProjectSourceType SourceType { get; set; }
        public int Count { get; set; }
    }
}