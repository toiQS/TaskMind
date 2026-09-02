// UnblockUserCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog + Notification — trước đây BlockUserCommand có
// AuditLog nhưng UnblockUserCommand (thao tác đối xứng) lại không có gì.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class UnblockUserCommand : IRequest<ServiceResult>
    {
        public Guid UserId { get; }
        public Guid ApproverAdminId { get; }

        public UnblockUserCommand(Guid userId, Guid approverAdminId)
        {
            UserId = userId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class UnblockUserHandler : IRequestHandler<UnblockUserCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public UnblockUserHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(UnblockUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user == null)
                return ServiceResult.NotFound("Không tìm thấy người dùng.");

            if (user.Status != EntityStatus.Blocked)
                return ServiceResult.Failure("Tài khoản hiện không ở trạng thái bị cấm.");

            user.UpdateStatus(EntityStatus.Active);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "UserUnblocked", nameof(User), user.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var notifResult = Notification.Create(
                user.Id,
                "Tài khoản đã được mở khoá",
                "Tài khoản của bạn đã được Admin hệ thống mở khoá và có thể hoạt động bình thường trở lại.",
                NotificationType.Success);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Mở khoá tài khoản thành công");
        }
    }
}
