// AdminRemoveProjectMemberCommand.cs
// [CẬP NHẬT - fix] Bổ sung Notification cho thành viên bị loại — trước đây chỉ có AuditLog, người bị
// gỡ khỏi dự án không hề được thông báo, không nhất quán với mọi luồng khác trong hệ thống (Join
// company/school, thay đổi kỹ năng, hoá đơn...) vốn luôn thông báo cho bên bị ảnh hưởng trực tiếp.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Projects
{
    public class AdminRemoveProjectMemberCommand : IRequest<ServiceResult>
    {
        public Guid ProjectId { get; }
        public Guid AccountId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public AdminRemoveProjectMemberCommand(Guid projectId, Guid accountId, Guid approverAdminId, string? reason = null)
        {
            ProjectId = projectId;
            AccountId = accountId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class AdminRemoveProjectMemberHandler : IRequestHandler<AdminRemoveProjectMemberCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public AdminRemoveProjectMemberHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(AdminRemoveProjectMemberCommand command, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

            if (project == null)
                return ServiceResult.NotFound("Không tìm thấy dự án.");

            var result = project.RemoveMember(command.AccountId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ProjectMemberRemovedByAdmin", nameof(Project), project.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var notifResult = Notification.Create(
                command.AccountId,
                "Bạn đã bị loại khỏi dự án",
                $"Bạn đã bị Admin hệ thống loại khỏi dự án \"{project.Title}\"." +
                (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Loại thành viên khỏi dự án thành công");
        }
    }
}
