using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Skills;

public sealed record CreateSkillByAdminCommand(SkillProposalRequest Request)
    : IRequest<ServiceResult<Guid>>;

public sealed class CreateSkillByAdminHandler
    : IRequestHandler<CreateSkillByAdminCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public CreateSkillByAdminHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(
        CreateSkillByAdminCommand command, CancellationToken ct)
    {
        var adminId = _session.GetUserId();
        if (!adminId.HasValue || adminId.Value == Guid.Empty ||
            !await _db.Admins.AnyAsync(item =>
                item.Id == adminId.Value && item.IsVerified && item.Status == EntityStatus.Active, ct))
            return ServiceResult<Guid>.Forbidden();

        if (!Enum.TryParse<SkillCategory>(command.Request.Category?.Trim(), true, out var category) ||
            !Enum.IsDefined(category))
            return ServiceResult<Guid>.Failure("Danh mục kỹ năng không hợp lệ.");
        var name = command.Request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<Guid>.Failure("Tên kỹ năng không được để trống.");
        var normalizedName = name.ToUpperInvariant();
        if (await _db.Skills.AnyAsync(item => item.NormalizedName == normalizedName, ct))
            return ServiceResult<Guid>.Failure("Kỹ năng đã tồn tại trong danh mục.");

        var created = Skill.CreateByAdmin(name, category);
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        var audit = AuditLog.Record(adminId.Value, "SkillCreatedByAdmin", nameof(Skill), created.Data.Id);
        if (!audit.IsSuccess || audit.Data is null)
            return ServiceResult<Guid>.Error(audit.Message);

        _db.Skills.Add(created.Data);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã tạo kỹ năng và duyệt trực tiếp.");
    }
}
