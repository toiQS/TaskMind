using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record CreateSystemAdminRequest(string Email, string Password, string CitizenId);
public sealed record SystemAdminDto(Guid AccountId, string Email, string Role);
public sealed record CreateSystemAdminCommand(CreateSystemAdminRequest Request)
    : IRequest<ServiceResult<SystemAdminDto>>;

public sealed class CreateSystemAdminHandler
    : IRequestHandler<CreateSystemAdminCommand, ServiceResult<SystemAdminDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public CreateSystemAdminHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<SystemAdminDto>> Handle(
        CreateSystemAdminCommand command, CancellationToken ct)
    {
        var actorId = _session.GetUserId();
        if (!actorId.HasValue || actorId.Value == Guid.Empty)
            return ServiceResult<SystemAdminDto>.Forbidden();
        var actor = await _db.Admins.SingleOrDefaultAsync(item => item.Id == actorId.Value, ct);
        if (actor is not { IsVerified: true, Status: EntityStatus.Active })
            return ServiceResult<SystemAdminDto>.Forbidden();

        var email = command.Request.Email?.Trim().ToLowerInvariant();
        var citizenId = command.Request.CitizenId?.Trim();
        if (!AuthRules.IsValidEmail(email))
            return ServiceResult<SystemAdminDto>.Failure("Email không hợp lệ.");
        if (!AuthRules.IsValidPassword(command.Request.Password))
            return ServiceResult<SystemAdminDto>.Failure(
                "Mật khẩu phải có ít nhất 8 ký tự và tối đa 72 byte UTF-8.");
        if (citizenId is null || !Profile.IsValidCitizenId(citizenId))
            return ServiceResult<SystemAdminDto>.Failure(
                "Mã số định danh phải gồm đúng 12 chữ số ASCII.");
        if (await _db.Accounts.AnyAsync(
                item => item.Profile.Email.ToUpper() == email!.ToUpper(), ct))
            return ServiceResult<SystemAdminDto>.Failure(
                "Email đã được sử dụng bởi một tài khoản trong hệ thống.");

        var created = Admin.Create(
            citizenId,
            email!,
            BCrypt.Net.BCrypt.HashPassword(command.Request.Password, workFactor: 12));
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<SystemAdminDto>.Failure(created.Message);
        var verified = created.Data.Verify();
        if (!verified.IsSuccess)
            return ServiceResult<SystemAdminDto>.Failure(verified.Message);
        var audit = AuditLog.Record(actor.Id, "SystemAdminCreated", nameof(Admin), created.Data.Id);
        if (!audit.IsSuccess || audit.Data is null)
            return ServiceResult<SystemAdminDto>.Error(audit.Message);

        _db.Admins.Add(created.Data);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<SystemAdminDto>.Success(
            new SystemAdminDto(created.Data.Id, created.Data.Profile.Email, AccountRole.Admin.ToString()),
            "Đã tạo tài khoản Admin hệ thống.");
    }
}
