using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities;

[Index(nameof(TokenHash), IsUnique = true)]
[Index(nameof(OrganizationId), nameof(MemberType), nameof(InvitationStatus))]
public sealed class OrganizationInvitation : AggregateRoot
{
    public Guid OrganizationId { get; private set; }
    public Guid InvitedUserId { get; private set; }
    public Guid InvitedByAccountId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public OrganizationMemberType MemberType { get; private set; }
    public OrganizationInvitationStatus InvitationStatus { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }

    private OrganizationInvitation() { }

    private OrganizationInvitation(
        Guid organizationId, Guid invitedUserId, Guid invitedByAccountId, string email,
        string tokenHash, OrganizationMemberType memberType, DateTime expiresAtUtc)
    {
        OrganizationId = organizationId;
        InvitedUserId = invitedUserId;
        InvitedByAccountId = invitedByAccountId;
        Email = email;
        TokenHash = tokenHash;
        MemberType = memberType;
        InvitationStatus = OrganizationInvitationStatus.Pending;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static Result<OrganizationInvitation> Create(
        Guid organizationId,
        Guid invitedUserId,
        Guid invitedByAccountId,
        string email,
        string tokenHash,
        OrganizationMemberType memberType,
        DateTime expiresAtUtc)
    {
        if (organizationId == Guid.Empty || invitedUserId == Guid.Empty || invitedByAccountId == Guid.Empty)
            return Result<OrganizationInvitation>.Failure("Thông tin tài khoản hoặc tổ chức không hợp lệ.");
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(tokenHash))
            return Result<OrganizationInvitation>.Failure("Email và mã lời mời là bắt buộc.");
        if (!Enum.IsDefined(memberType) ||
            (memberType == OrganizationMemberType.Staff && organizationId == Guid.Empty))
            return Result<OrganizationInvitation>.Failure("Loại thành viên không hợp lệ.");
        if (expiresAtUtc <= DateTime.UtcNow)
            return Result<OrganizationInvitation>.Failure("Thời hạn lời mời phải nằm trong tương lai.");
        return Result<OrganizationInvitation>.Success(new OrganizationInvitation(
            organizationId, invitedUserId, invitedByAccountId,
            email.Trim().ToLowerInvariant(), tokenHash, memberType, expiresAtUtc));
    }

    public Result Accept(Guid userId, string email, DateTime nowUtc)
    {
        if (InvitationStatus != OrganizationInvitationStatus.Pending)
            return Result.Failure("Lời mời không còn ở trạng thái chờ xác nhận.");
        if (ExpiresAtUtc <= nowUtc)
        {
            InvitationStatus = OrganizationInvitationStatus.Expired;
            return Result.Failure("Lời mời đã hết hạn.");
        }
        if (userId != InvitedUserId ||
            !string.Equals(email, Email, StringComparison.OrdinalIgnoreCase))
            return Result.Failure("Lời mời không dành cho tài khoản hiện tại.");
        InvitationStatus = OrganizationInvitationStatus.Accepted;
        AcceptedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Renew(string tokenHash, DateTime expiresAtUtc)
    {
        if (InvitationStatus != OrganizationInvitationStatus.Pending)
            return Result.Failure("Chỉ lời mời đang chờ mới có thể gửi lại.");
        if (string.IsNullOrWhiteSpace(tokenHash) || expiresAtUtc <= DateTime.UtcNow)
            return Result.Failure("Token hoặc thời hạn lời mời không hợp lệ.");
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        return Result.Success();
    }
}
