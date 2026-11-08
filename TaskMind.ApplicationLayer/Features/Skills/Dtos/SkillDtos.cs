namespace TaskMind.ApplicationLayer.Features.Skills.Dtos;

public sealed class SkillCatalogItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public IReadOnlyCollection<Guid> RelatedSkillIds { get; init; } = Array.Empty<Guid>();
}

public sealed class UserSkillDto
{
    public Guid SkillId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public int EndorsementCount { get; init; }
    public IReadOnlyCollection<Guid> EndorserIds { get; init; } = Array.Empty<Guid>();
}

public sealed class UserSkillsDto
{
    public IReadOnlyCollection<UserSkillDto> Items { get; init; } = Array.Empty<UserSkillDto>();
}

public sealed class SkillHistoryItemDto
{
    public Guid Id { get; init; }
    public Guid SkillId { get; init; }
    public string? SkillName { get; init; }
    public string ChangeSource { get; init; } = string.Empty;
    public string? LevelBefore { get; init; }
    public string? LevelAfter { get; init; }
    public bool IsNewlyAdded { get; init; }
    public Guid ResponsibleAccountId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? ProjectId { get; init; }
    public DateTimeOffset? TenureStartUtc { get; init; }
    public DateTimeOffset? TenureEndUtc { get; init; }
    public string EvidenceDescription { get; init; } = string.Empty;
    public Guid? RelatedSubmissionId { get; init; }
    public Guid? RelatedRequestId { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed class PagedResultDto<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}