namespace TaskMind.ApplicationLayer.Features.Auths.Dtos;

public sealed class PersonalSubmissionsDto
{
    public IReadOnlyCollection<PersonalSubmissionDto> Items { get; init; } = Array.Empty<PersonalSubmissionDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class PersonalSubmissionDto
{
    public Guid Id { get; init; }
    public Guid TestPaperId { get; init; }
    public string TestPaperTitle { get; init; } = string.Empty;
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public decimal? Score { get; init; }
    public DateTime? SubmittedAtUtc { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class PersonalCertificatesDto
{
    public IReadOnlyCollection<PersonalCertificateDto> Items { get; init; } = Array.Empty<PersonalCertificateDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class PersonalCertificateDto
{
    public Guid Id { get; init; }
    public Guid? SubmissionId { get; init; }
    public string VerificationCode { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
}

public sealed class PersonalJobApplicationsDto
{
    public IReadOnlyCollection<PersonalJobApplicationDto> Items { get; init; } = Array.Empty<PersonalJobApplicationDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class PersonalJobApplicationDto
{
    public Guid Id { get; init; }
    public Guid JobPostingId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime AppliedAtUtc { get; init; }
}

public sealed class PersonalNotificationsDto
{
    public IReadOnlyCollection<PersonalNotificationDto> Items { get; init; } = Array.Empty<PersonalNotificationDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class PersonalNotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class PersonalSkillLevelUpRequestsDto
{
    public IReadOnlyCollection<PersonalSkillLevelUpRequestDto> Items { get; init; } =
        Array.Empty<PersonalSkillLevelUpRequestDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class PersonalSkillLevelUpRequestDto
{
    public Guid Id { get; init; }
    public Guid SkillId { get; init; }
    public string SkillName { get; init; } = string.Empty;
    public string CurrentLevel { get; init; } = string.Empty;
    public Guid ApproverAccountId { get; init; }
    public string RequestType { get; init; } = string.Empty;
    public Guid? SubmissionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
}
