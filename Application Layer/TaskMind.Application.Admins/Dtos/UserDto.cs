namespace TaskMind.Applications.Admins.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>AccountRole: User/Student/Staff/Teacher/Admin/AdminCompany/AdminSchool</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>EntityStatus: Active/Paused (Tạm khoá)/Blocked (Bị cấm)/Deleted</summary>
        public string Status { get; set; } = string.Empty;

        public bool IsVerified { get; set; }
        public DateTime JoinedDateUtc { get; set; }

        /// <summary>Số kỹ năng đã khai báo trong SkillProfile (mục 4.3) — dùng cho cột thống kê nhanh ở UserView.</summary>
        public int SkillCount { get; set; }

        /// <summary>Số dự án (Project) mà tài khoản này đang/đã tham gia (qua ProjectMember).</summary>
        public int ProjectCount { get; set; }
    }

    public class UserDetailDto : UserDto
    {
        public string Bio { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? LastActiveDateUtc { get; set; }

        public List<UserSkillItemDto> Skills { get; set; } = new();
        public List<UserProjectHistoryItemDto> ProjectHistory { get; set; } = new();
        public List<AuditLogEntryDto> AuditLogs { get; set; } = new();
    }

    /// <summary>Một dòng kỹ năng trong hồ sơ SkillProfile của User (mục 4.3).</summary>
    public class UserSkillItemDto
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;

        /// <summary>SkillCategory: ProgrammingLanguage/Framework/SoftSkill/Tool/Other</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>SkillLevel: Beginner/Intermediate/Advanced/Expert</summary>
        public string Level { get; set; } = string.Empty;
        public int EndorsementCount { get; set; }
    }

    /// <summary>Một dòng lịch sử tham gia dự án (Project + ProjectMember) của User.</summary>
    public class UserProjectHistoryItemDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>ProjectRole: Owner/TechnicalLeader/ProjectManager/QaQc/Developer/Intern</summary>
        public string ProjectRole { get; set; } = string.Empty;

        /// <summary>ProjectSourceType: Company/School/OpenSource</summary>
        public string ProjectSource { get; set; } = string.Empty;
        public DateTime StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public bool IsOngoing { get; set; }
    }

    /// <summary>Một dòng nhật ký kiểm toán (AuditTrail) liên quan tới thực thể đang xem (mục 5.7).</summary>
    public class AuditLogEntryDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = string.Empty;

        /// <summary>Create/Update/Delete</summary>
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTimeOffset TimestampUtc { get; set; }
    }
}
