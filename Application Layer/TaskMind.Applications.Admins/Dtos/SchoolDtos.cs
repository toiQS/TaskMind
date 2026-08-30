using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class SchoolListItemDto
    {
        public Guid Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public EntityStatus Status { get; set; }
        public string MembershipPackage { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
    }

    public class SchoolDetailDto
    {
        public Guid Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Address Address { get; set; } = new();
        public bool IsVerified { get; set; }
        public EntityStatus Status { get; set; }
        public string MembershipPackage { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public int ActiveTeacherCount { get; set; }
        public int ActiveStudentCount { get; set; }
        public int TotalProjectCount { get; set; }
    }

    public class GetSchoolsFilter
    {
        public bool? IsVerified { get; set; }
        public EntityStatus? Status { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}