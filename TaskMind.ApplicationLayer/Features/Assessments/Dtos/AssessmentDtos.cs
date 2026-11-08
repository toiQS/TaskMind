namespace TaskMind.ApplicationLayer.Features.Assessments.Dtos;

public sealed class TestPaperDto
{
    public Guid Id { get; init; }
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int DurationMinutes { get; init; }
}

public sealed class SubmissionDto
{
    public Guid Id { get; init; }
    public Guid TestPaperId { get; init; }
    public string TestPaperTitle { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public decimal? Score { get; init; }
    public DateTime? SubmittedAtUtc { get; init; }
    public bool IsPassed { get; init; }
    public bool IsGraded { get; init; }
}

public sealed class AssessmentSubmissionSummaryDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal? Score { get; init; }
    public DateTime? SubmittedAtUtc { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class CertificateVerificationDto
{
    public bool IsValid { get; init; }
    public string VerificationCode { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string? TestPaperTitle { get; init; }
    public DateTime IssuedAtUtc { get; init; }
}

public sealed class PagedAssessmentDto<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
