using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Assessments.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Assessments;

public sealed record ListTestPapersQuery(
    TestOwnerType OwnerType,
    Guid OwnerId,
    int Page = 1,
    int PageSize = 20) : IRequest<ServiceResult<PagedAssessmentDto<TestPaperDto>>>;

public sealed class ListTestPapersHandler :
    IRequestHandler<ListTestPapersQuery, ServiceResult<PagedAssessmentDto<TestPaperDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListTestPapersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<PagedAssessmentDto<TestPaperDto>>> Handle(
        ListTestPapersQuery query,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(query.OwnerType) || query.OwnerId == Guid.Empty)
            return ServiceResult<PagedAssessmentDto<TestPaperDto>>.Failure("Chủ sở hữu bài kiểm tra không hợp lệ.");
        if (!AssessmentQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PagedAssessmentDto<TestPaperDto>>.Failure(error);
        if (!await AssessmentAuthorization.IsOwnerActiveAsync(
                _dbContext, query.OwnerType, query.OwnerId, cancellationToken))
            return ServiceResult<PagedAssessmentDto<TestPaperDto>>.NotFound("Không tìm thấy đơn vị sở hữu.");

        var papers = _dbContext.TestPapers.AsNoTracking()
            .Where(item => item.OwnerType == query.OwnerType &&
                           item.OwnerId == query.OwnerId &&
                           item.Status == EntityStatus.Active);
        var totalCount = await papers.CountAsync(cancellationToken);
        var items = await papers
            .OrderBy(item => item.Title)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(item => new TestPaperDto
            {
                Id = item.Id,
                OwnerType = item.OwnerType.ToString(),
                OwnerId = item.OwnerId,
                Title = item.Title,
                DurationMinutes = item.DurationMinutes
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedAssessmentDto<TestPaperDto>>.Success(
            new PagedAssessmentDto<TestPaperDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
    }
}

public sealed record GetTestPaperQuery(Guid TestPaperId) : IRequest<ServiceResult<TestPaperDto>>;

public sealed class GetTestPaperHandler : IRequestHandler<GetTestPaperQuery, ServiceResult<TestPaperDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTestPaperHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<TestPaperDto>> Handle(
        GetTestPaperQuery query,
        CancellationToken cancellationToken)
    {
        var paper = await _dbContext.TestPapers.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == query.TestPaperId && item.Status == EntityStatus.Active,
                cancellationToken);
        if (paper is null)
            return ServiceResult<TestPaperDto>.NotFound("Không tìm thấy bài kiểm tra.");

        if (!await AssessmentAuthorization.IsOwnerActiveAsync(
                _dbContext, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<TestPaperDto>.NotFound("Không tìm thấy bài kiểm tra.");

        return ServiceResult<TestPaperDto>.Success(AssessmentMappings.ToDto(paper));
    }
}

public sealed record ListTestPaperSubmissionsQuery(
    Guid TestPaperId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>>;

public sealed class ListTestPaperSubmissionsHandler :
    IRequestHandler<ListTestPaperSubmissionsQuery, ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public ListTestPaperSubmissionsHandler(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>> Handle(
        ListTestPaperSubmissionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!AssessmentQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>.Failure(error);

        var status = query.Status?.Trim().ToLowerInvariant();
        if (status is not null && status is not ("draft" or "submitted" or "graded"))
            return ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>.Failure(
                "Status phải là draft, submitted hoặc graded.");

        var paper = await _dbContext.TestPapers.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == query.TestPaperId && item.Status == EntityStatus.Active,
            cancellationToken);
        if (paper is null)
            return ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>.NotFound(
                "Không tìm thấy bài kiểm tra.");

        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>.Forbidden(
                accountResult.Message);
        if (!await AssessmentAuthorization.CanManageOwnerAsync(
                _dbContext, accountResult.Data, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>.Forbidden(
                "Bạn không có quyền xem bài nộp của bài kiểm tra này.");

        var submissions = _dbContext.Submissions.AsNoTracking()
            .Where(item => item.TestPaperId == paper.Id);
        submissions = status switch
        {
            "draft" => submissions.Where(item => item.SubmittedAtUtc == null),
            "submitted" => submissions.Where(item => item.SubmittedAtUtc != null && item.Score == null),
            "graded" => submissions.Where(item => item.Score != null),
            _ => submissions
        };

        var totalCount = await submissions.CountAsync(cancellationToken);
        var items = await submissions
            .OrderByDescending(item => item.SubmittedAtUtc)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(item => new AssessmentSubmissionSummaryDto
            {
                Id = item.Id,
                UserId = item.UserId,
                Score = item.Score,
                SubmittedAtUtc = item.SubmittedAtUtc,
                Status = item.Score != null
                    ? "Graded"
                    : item.SubmittedAtUtc != null
                        ? "Submitted"
                        : "Draft"
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedAssessmentDto<AssessmentSubmissionSummaryDto>>.Success(
            new PagedAssessmentDto<AssessmentSubmissionSummaryDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
    }
}

internal static class AssessmentQueryRules
{
    public static bool TryGetPaging(int page, int pageSize, out int skip, out string error)
    {
        skip = 0;
        error = string.Empty;
        if (page < 1 || pageSize is < 1 or > 100)
        {
            error = "Page phải từ 1 trở lên và PageSize phải từ 1 đến 100.";
            return false;
        }

        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            error = "Page vượt quá giới hạn hỗ trợ.";
            return false;
        }

        skip = (int)offset;
        return true;
    }
}
