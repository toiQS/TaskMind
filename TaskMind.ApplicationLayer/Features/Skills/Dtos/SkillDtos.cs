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

public sealed class PagedResultDto<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}