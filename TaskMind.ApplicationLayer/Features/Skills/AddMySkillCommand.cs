using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Skills.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Skills;

public sealed record AddMySkillCommand(Guid SkillId, string Level)
    : IRequest<ServiceResult<UserSkillDto>>;

public sealed class AddMySkillHandler : IRequestHandler<AddMySkillCommand, ServiceResult<UserSkillDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public AddMySkillHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<UserSkillDto>> Handle(
        AddMySkillCommand command,
        CancellationToken cancellationToken)
    {
        if (command.SkillId == Guid.Empty ||
            !Enum.TryParse<SkillLevel>(command.Level?.Trim(), ignoreCase: true, out var level) ||
            !Enum.IsDefined(level) ||
            !string.Equals(Enum.GetName(level), command.Level?.Trim(), StringComparison.OrdinalIgnoreCase))
            return ServiceResult<UserSkillDto>.Failure("SkillId hoặc cấp độ kỹ năng không hợp lệ.");

        var accountResult = await SkillAccountResolver.ResolveAsync(_dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<UserSkillDto>.Forbidden(accountResult.Message);

        var skill = await _dbContext.Skills.SingleOrDefaultAsync(
            candidate => candidate.Id == command.SkillId &&
                         candidate.IsApproved &&
                         candidate.Status == EntityStatus.Active,
            cancellationToken);
        if (skill is null)
            return ServiceResult<UserSkillDto>.NotFound("Không tìm thấy kỹ năng đã được duyệt trong danh mục.");

        var profile = await _dbContext.SkillProfiles
            .Include(skillProfile => skillProfile.Records)
            .SingleOrDefaultAsync(
                skillProfile => skillProfile.UserId == accountResult.Data.UserId,
                cancellationToken);
        var isNewProfile = profile is null;
        if (profile is null)
        {
            var profileResult = SkillProfile.Create(accountResult.Data.UserId);
            if (!profileResult.IsSuccess)
                return ServiceResult<UserSkillDto>.Failure(profileResult.Message);
            profile = profileResult.Data;
            _dbContext.SkillProfiles.Add(profile);
        }

        var declareResult = profile.DeclareSkill(command.SkillId, level);
        if (!declareResult.IsSuccess)
            return ServiceResult<UserSkillDto>.Failure(declareResult.Message);

        var historyResult = SkillHistoryEntry.Record(
            userId: accountResult.Data.UserId,
            skillId: skill.Id,
            changeSource: SkillChangeSource.UserInitiated,
            responsibleAccountId: accountResult.Data.AccountId,
            outcome: SkillHistoryOutcome.Applied,
            levelAfter: level,
            isNewlyAdded: true);
        if (!historyResult.IsSuccess || historyResult.Data is null)
            return ServiceResult<UserSkillDto>.Failure(historyResult.Message);

        var auditResult = AuditLog.Record(
            accountResult.Data.AccountId,
            "SkillAdded",
            nameof(SkillProfile),
            profile.Id);
        if (!auditResult.IsSuccess || auditResult.Data is null)
            return ServiceResult<UserSkillDto>.Failure(auditResult.Message);

        _dbContext.SkillHistoryEntries.Add(historyResult.Data);
        _dbContext.AuditLogs.Add(auditResult.Data);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<UserSkillDto>.Failure(
                "Hồ sơ kỹ năng vừa được cập nhật. Hãy tải lại và thử lại.");
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            return ServiceResult<UserSkillDto>.Failure(
                isNewProfile
                    ? "Hồ sơ kỹ năng vừa được tạo bởi một yêu cầu khác. Hãy tải lại và thử lại."
                    : "Kỹ năng vừa được thêm bởi một yêu cầu khác.");
        }

        return ServiceResult<UserSkillDto>.Success(new UserSkillDto
        {
            SkillId = skill.Id,
            Name = skill.SkillName,
            Category = skill.Category.ToString(),
            Level = level.ToString(),
            EndorsementCount = 0,
            EndorserIds = Array.Empty<Guid>()
        }, "Đã thêm kỹ năng vào hồ sơ.");
    }
}