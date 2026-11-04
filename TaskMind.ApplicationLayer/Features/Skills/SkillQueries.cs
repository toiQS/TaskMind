using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Skills.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Skills;

public sealed record GetSkillCatalogQuery(
    string? Keyword = null,
    string? Category = null,
    int Page = 1,
    int PageSize = 20) : IRequest<ServiceResult<PagedResultDto<SkillCatalogItemDto>>>;

public sealed class GetSkillCatalogHandler :
    IRequestHandler<GetSkillCatalogQuery, ServiceResult<PagedResultDto<SkillCatalogItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSkillCatalogHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<PagedResultDto<SkillCatalogItemDto>>> Handle(
        GetSkillCatalogQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100)
            return ServiceResult<PagedResultDto<SkillCatalogItemDto>>.Failure(
                "Page phải từ 1 trở lên và PageSize phải từ 1 đến 100.");
        var skip = ((long)query.Page - 1) * query.PageSize;
        if (skip > int.MaxValue)
            return ServiceResult<PagedResultDto<SkillCatalogItemDto>>.Failure("Page vượt quá giới hạn hỗ trợ.");

        SkillCategory? category = null;
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            if (!Enum.TryParse<SkillCategory>(query.Category.Trim(), ignoreCase: true, out var parsedCategory) ||
                !Enum.IsDefined(parsedCategory) ||
                !string.Equals(
                    Enum.GetName(parsedCategory),
                    query.Category.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                return ServiceResult<PagedResultDto<SkillCatalogItemDto>>.Failure("Danh mục kỹ năng không hợp lệ.");
            category = parsedCategory;
        }

        var skills = _dbContext.Skills
            .AsNoTracking()
            .Where(skill => skill.IsApproved && skill.Status == EntityStatus.Active);
        if (category.HasValue)
            skills = skills.Where(skill => skill.Category == category.Value);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToUpperInvariant();
            skills = skills.Where(skill => skill.SkillName.ToUpper().Contains(keyword));
        }

        var totalCount = await skills.CountAsync(cancellationToken);
        var skillsOnPage = await skills
            .OrderBy(skill => skill.NormalizedName)
            .ThenBy(skill => skill.Id)
            .Skip((int)skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var items = skillsOnPage.Select(skill => new SkillCatalogItemDto
        {
            Id = skill.Id,
            Name = skill.SkillName,
            Category = skill.Category.ToString(),
            RelatedSkillIds = skill.RelatedSkillIds.ToArray()
        }).ToArray();

        return ServiceResult<PagedResultDto<SkillCatalogItemDto>>.Success(
            new PagedResultDto<SkillCatalogItemDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
    }
}

public sealed record GetMySkillsQuery : IRequest<ServiceResult<UserSkillsDto>>;

public sealed class GetMySkillsHandler :
    IRequestHandler<GetMySkillsQuery, ServiceResult<UserSkillsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public GetMySkillsHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<UserSkillsDto>> Handle(
        GetMySkillsQuery query,
        CancellationToken cancellationToken)
    {
        var accountResult = await SkillAccountResolver.ResolveAsync(_dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess)
            return ServiceResult<UserSkillsDto>.Forbidden(accountResult.Message);

        var profile = await _dbContext.SkillProfiles
            .AsNoTracking()
            .Include(skillProfile => skillProfile.Records)
            .SingleOrDefaultAsync(skillProfile => skillProfile.UserId == accountResult.Data.UserId, cancellationToken);
        if (profile is null || profile.Records.Count == 0)
            return ServiceResult<UserSkillsDto>.Success(new UserSkillsDto());

        var skillIds = profile.Records.Select(record => record.SkillId).ToArray();
        var skillsById = await _dbContext.Skills
            .AsNoTracking()
            .Where(skill => skillIds.Contains(skill.Id))
            .ToDictionaryAsync(skill => skill.Id, cancellationToken);

        var items = profile.Records
            .Where(record => skillsById.ContainsKey(record.SkillId))
            .Select(record =>
            {
                var skill = skillsById[record.SkillId];
                return new UserSkillDto
                {
                    SkillId = skill.Id,
                    Name = skill.SkillName,
                    Category = skill.Category.ToString(),
                    Level = record.Level.ToString(),
                    EndorsementCount = record.EndorsementCount,
                    EndorserIds = record.EndorserIds.ToArray()
                };
            })
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ServiceResult<UserSkillsDto>.Success(new UserSkillsDto { Items = items });
    }
}

internal sealed record ResolvedSkillAccount(Guid AccountId, Guid UserId);

internal static class SkillAccountResolver
{
    public static async Task<TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<ResolvedSkillAccount>> ResolveAsync(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider,
        CancellationToken cancellationToken)
    {
        var accountId = sessionProvider.GetUserId();
        if (!accountId.HasValue || accountId.Value == Guid.Empty)
            return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<ResolvedSkillAccount>.Failure(
                "Không xác định được tài khoản hiện tại.");

        var account = await dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Id == accountId.Value,
            cancellationToken);
        if (account is null || !account.IsVerified || account.Status != EntityStatus.Active)
            return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<ResolvedSkillAccount>.Failure(
                "Tài khoản không đủ điều kiện truy cập hồ sơ kỹ năng.");

        var userId = account switch
        {
            User user => user.Id,
            Staff staff when staff.IsActive => staff.LinkedUserId,
            Student student when student.IsActive => student.LinkedUserId,
            Teacher teacher when teacher.IsActive => teacher.LinkedUserId,
            AdminCompany adminCompany => adminCompany.LinkedUserId,
            AdminSchool adminSchool => adminSchool.LinkedUserId,
            _ => Guid.Empty
        };
        if (userId == Guid.Empty ||
            !await dbContext.Users.AnyAsync(
                user => user.Id == userId && user.IsVerified && user.Status == EntityStatus.Active,
                cancellationToken))
            return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<ResolvedSkillAccount>.Failure(
                "Tài khoản hiện tại không được liên kết với hồ sơ User đang hoạt động.");

        return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<ResolvedSkillAccount>.Success(
            new ResolvedSkillAccount(account.Id, userId));
    }
}