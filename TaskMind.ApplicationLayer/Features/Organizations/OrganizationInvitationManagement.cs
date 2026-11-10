using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Organizations;

public sealed record InviteOrganizationMemberCommand(
    Guid OrganizationId, string Email, OrganizationMemberType MemberType)
    : IRequest<ServiceResult<Guid>>;
public sealed record AcceptOrganizationInvitationCommand(string Token)
    : IRequest<ServiceResult<Guid>>;

public sealed class InviteOrganizationMemberHandler :
    IRequestHandler<InviteOrganizationMemberCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    private readonly IEmailSender _emailSender;

    public InviteOrganizationMemberHandler(
        IApplicationDbContext db, ICurrentSessionProvider session, IEmailSender emailSender)
    {
        _db = db;
        _session = session;
        _emailSender = emailSender;
    }

    public async Task<ServiceResult<Guid>> Handle(
        InviteOrganizationMemberCommand command, CancellationToken ct)
    {
        if (!Enum.IsDefined(command.MemberType) ||
            !OrganizationRules.IsValidEmail(command.Email))
            return ServiceResult<Guid>.Failure("Loại thành viên hoặc email không hợp lệ.");
        var actorId = _session.GetUserId();
        if (!actorId.HasValue || actorId.Value == Guid.Empty)
            return ServiceResult<Guid>.Unauthorized("Không xác định được tài khoản hiện tại.");
        var actor = await _db.Accounts.SingleOrDefaultAsync(item => item.Id == actorId.Value, ct);
        if (actor is not { IsVerified: true, Status: EntityStatus.Active })
            return ServiceResult<Guid>.Forbidden("Tài khoản không đủ điều kiện gửi lời mời.");

        var isAuthorized = command.MemberType switch
        {
            OrganizationMemberType.Staff =>
                actor is AdminCompany companyAdmin &&
                companyAdmin.CompanyId == command.OrganizationId &&
                await _db.Companies.AnyAsync(item => item.Id == command.OrganizationId &&
                    item.IsVerified && item.Status == EntityStatus.Active, ct),
            OrganizationMemberType.Teacher or OrganizationMemberType.Student =>
                actor is AdminSchool schoolAdmin &&
                schoolAdmin.SchoolId == command.OrganizationId &&
                await _db.Schools.AnyAsync(item => item.Id == command.OrganizationId &&
                    item.IsVerified && item.Status == EntityStatus.Active, ct),
            _ => false
        };
        if (!isAuthorized)
            return ServiceResult<Guid>.Forbidden("Bạn không có quyền mời thành viên vào tổ chức này.");

        var email = command.Email.Trim().ToLowerInvariant();
        var invitee = await _db.Users.SingleOrDefaultAsync(item =>
            item.Profile.Email.ToLower() == email &&
            item.IsVerified && item.Status == EntityStatus.Active, ct);
        if (invitee is null)
            return ServiceResult<Guid>.NotFound("Chỉ có thể mời người dùng đã đăng ký và xác minh email.");
        var alreadyMember = command.MemberType switch
        {
            OrganizationMemberType.Staff => await _db.Staffs.AnyAsync(item =>
                item.CompanyId == command.OrganizationId && item.LinkedUserId == invitee.Id && item.IsActive, ct),
            OrganizationMemberType.Teacher => await _db.Teachers.AnyAsync(item =>
                item.SchoolId == command.OrganizationId && item.LinkedUserId == invitee.Id && item.IsActive, ct),
            OrganizationMemberType.Student => await _db.Students.AnyAsync(item =>
                item.SchoolId == command.OrganizationId && item.LinkedUserId == invitee.Id && item.IsActive, ct),
            _ => false
        };
        if (alreadyMember)
            return ServiceResult<Guid>.Failure("Người dùng đã là thành viên đang hoạt động của tổ chức.");
        var invitation = await _db.OrganizationInvitations.SingleOrDefaultAsync(item =>
                item.OrganizationId == command.OrganizationId &&
                item.InvitedUserId == invitee.Id &&
                item.MemberType == command.MemberType &&
                item.InvitationStatus == OrganizationInvitationStatus.Pending, ct);

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var expiresAt = DateTime.UtcNow.AddDays(7);
        if (invitation is null)
        {
            var invitationResult = OrganizationInvitation.Create(
                command.OrganizationId, invitee.Id, actor.Id, email, tokenHash, command.MemberType, expiresAt);
            if (!invitationResult.IsSuccess || invitationResult.Data is null)
                return ServiceResult<Guid>.Failure(invitationResult.Message);
            invitation = invitationResult.Data;
            _db.OrganizationInvitations.Add(invitation);
        }
        else
        {
            var renewed = invitation.Renew(tokenHash, expiresAt);
            if (!renewed.IsSuccess) return ServiceResult<Guid>.Failure(renewed.Message);
        }
        var audit = AuditLog.Record(actor.Id, "OrganizationMemberInvited",
            nameof(OrganizationInvitation), invitation.Id);
        if (!audit.IsSuccess || audit.Data is null)
            return ServiceResult<Guid>.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);

        var safeToken = HtmlEncoder.Default.Encode(token);
        var sent = await _emailSender.SendAsync(
            email,
            "Lời mời tham gia TaskMind",
            $"<p>Bạn được mời tham gia tổ chức trên TaskMind với vai trò {command.MemberType}.</p>" +
            $"<p>Mã lời mời có hiệu lực đến {expiresAt:O}:</p><p><strong>{safeToken}</strong></p>",
            ct);
        if (!sent)
            return ServiceResult<Guid>.Error(
                "Lời mời đã được lưu nhưng email chưa gửi được. Hãy thử gửi lại sau.");
        return ServiceResult<Guid>.Success(invitation.Id, "Đã gửi lời mời qua email.");
    }
}

public sealed class AcceptOrganizationInvitationHandler :
    IRequestHandler<AcceptOrganizationInvitationCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public AcceptOrganizationInvitationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(AcceptOrganizationInvitationCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Token) || command.Token.Length != 64 ||
            command.Token.Any(character => !Uri.IsHexDigit(character)))
            return ServiceResult<Guid>.Failure("Token lời mời không hợp lệ.");
        var accountId = _session.GetUserId();
        if (!accountId.HasValue || accountId.Value == Guid.Empty)
            return ServiceResult<Guid>.Unauthorized("Cần đăng nhập bằng tài khoản User được mời.");
        var user = await _db.Users.SingleOrDefaultAsync(item =>
            item.Id == accountId.Value && item.IsVerified && item.Status == EntityStatus.Active, ct);
        if (user is null)
            return ServiceResult<Guid>.Forbidden("Chỉ tài khoản User đã xác minh mới có thể chấp nhận lời mời.");
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(command.Token)));
        var invitation = await _db.OrganizationInvitations.SingleOrDefaultAsync(
            item => item.TokenHash == tokenHash, ct);
        if (invitation is null)
            return ServiceResult<Guid>.NotFound("Không tìm thấy lời mời.");
        if (!string.Equals(user.Profile.Email, invitation.Email, StringComparison.OrdinalIgnoreCase) ||
            invitation.InvitedUserId != user.Id)
            return ServiceResult<Guid>.Forbidden("Lời mời không dành cho tài khoản hiện tại.");
        if (await _db.Staffs.AnyAsync(item => item.LinkedUserId == user.Id && item.IsActive, ct) ||
            await _db.Teachers.AnyAsync(item => item.LinkedUserId == user.Id && item.IsActive, ct) ||
            await _db.Students.AnyAsync(item => item.LinkedUserId == user.Id && item.IsActive, ct))
            return ServiceResult<Guid>.Failure(
                "Tài khoản đã có một vai trò thành viên đang hoạt động. Hãy rời vai trò hiện tại trước.");

        Account member;
        switch (invitation.MemberType)
        {
            case OrganizationMemberType.Staff:
            {
                if (!await _db.Companies.AnyAsync(item => item.Id == invitation.OrganizationId &&
                        item.IsVerified && item.Status == EntityStatus.Active, ct))
                    return ServiceResult<Guid>.Failure("Công ty đã ngừng hoạt động.");
                if (await _db.Staffs.AnyAsync(item => item.CompanyId == invitation.OrganizationId &&
                        item.LinkedUserId == user.Id && item.IsActive, ct))
                    return ServiceResult<Guid>.Failure("Bạn đã là nhân sự đang hoạt động của công ty.");
                var created = Staff.Create(user.Profile.CitizenId, user.Profile.Email,
                    user.Security.PasswordHash, user.Id, invitation.OrganizationId);
                if (!created.IsSuccess || created.Data is null) return ServiceResult<Guid>.Failure(created.Message);
                member = created.Data;
                break;
            }
            case OrganizationMemberType.Teacher:
            {
                if (!await _db.Schools.AnyAsync(item => item.Id == invitation.OrganizationId &&
                        item.IsVerified && item.Status == EntityStatus.Active, ct))
                    return ServiceResult<Guid>.Failure("Cơ sở đào tạo đã ngừng hoạt động.");
                if (await _db.Teachers.AnyAsync(item => item.SchoolId == invitation.OrganizationId &&
                        item.LinkedUserId == user.Id && item.IsActive, ct))
                    return ServiceResult<Guid>.Failure("Bạn đã là giảng viên đang hoạt động của cơ sở đào tạo.");
                var created = Teacher.Create(user.Profile.CitizenId, user.Profile.Email,
                    user.Security.PasswordHash, user.Id, invitation.OrganizationId);
                if (!created.IsSuccess || created.Data is null) return ServiceResult<Guid>.Failure(created.Message);
                member = created.Data;
                break;
            }
            case OrganizationMemberType.Student:
            {
                if (!await _db.Schools.AnyAsync(item => item.Id == invitation.OrganizationId &&
                        item.IsVerified && item.Status == EntityStatus.Active, ct))
                    return ServiceResult<Guid>.Failure("Cơ sở đào tạo đã ngừng hoạt động.");
                if (await _db.Students.AnyAsync(item => item.SchoolId == invitation.OrganizationId &&
                        item.LinkedUserId == user.Id && item.IsActive, ct))
                    return ServiceResult<Guid>.Failure("Bạn đã là học viên đang hoạt động của cơ sở đào tạo.");
                var created = Student.Create(user.Profile.CitizenId, user.Profile.Email,
                    user.Security.PasswordHash, user.Id, invitation.OrganizationId);
                if (!created.IsSuccess || created.Data is null) return ServiceResult<Guid>.Failure(created.Message);
                member = created.Data;
                break;
            }
            default:
                return ServiceResult<Guid>.Failure("Vai trò lời mời không hợp lệ.");
        }

        var verified = member.Verify();
        if (!verified.IsSuccess) return ServiceResult<Guid>.Failure(verified.Message);
        var profileUpdated = member.Profile.UpdateProfile(
            user.Profile.FirstName, user.Profile.LastName, user.Profile.NickName,
            user.Profile.Bio, user.Profile.ImageUrl, user.Profile.PhoneNumber,
            user.Profile.Address.Street, user.Profile.Address.City, user.Profile.Address.Country);
        if (!profileUpdated.IsSuccess) return ServiceResult<Guid>.Failure(profileUpdated.Message);
        var accepted = invitation.Accept(user.Id, user.Profile.Email, DateTime.UtcNow);
        if (!accepted.IsSuccess)
        {
            if (invitation.InvitationStatus == OrganizationInvitationStatus.Expired)
                await _db.SaveChangesAsync(ct);
            return ServiceResult<Guid>.Failure(accepted.Message);
        }
        switch (member)
        {
            case Staff staff: _db.Staffs.Add(staff); break;
            case Teacher teacher: _db.Teachers.Add(teacher); break;
            case Student student: _db.Students.Add(student); break;
        }
        var notification = Notification.Create(
            user.Id, "Lời mời đã được chấp nhận",
            $"Bạn đã tham gia tổ chức với vai trò {invitation.MemberType}.",
            NotificationType.Success);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult<Guid>.Error(notification.Message);
        _db.Notifications.Add(notification.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is Npgsql.PostgresException
            {
                SqlState: Npgsql.PostgresErrorCodes.UniqueViolation
            })
        {
            return ServiceResult<Guid>.Failure(
                "Tài khoản vừa được liên kết với một vai trò tổ chức khác. Tải lại trạng thái và thử lại.");
        }
        return ServiceResult<Guid>.Success(member.Id, "Đã chấp nhận lời mời.");
    }
}
