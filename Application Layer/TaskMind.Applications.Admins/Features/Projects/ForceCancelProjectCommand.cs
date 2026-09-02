// ForceCancelProjectCommand.cs
// [CẬP NHẬT - fix] Bổ sung Notification cho toàn bộ thành viên đang hoạt động — trước đây
// ForceCancelProjectCommand hoàn toàn không thông báo cho ai, trong khi ForcePauseProjectCommand
// (tác động NHẸ hơn) lại có thông báo đầy đủ cho member. Huỷ dự án là hành động không thể hoàn tác,
// nên member càng cần được biết. Cần .Include(p => p.Members) — bản gốc không có, khiến Members luôn
// rỗng khi duyệt vòng lặp thông báo.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Projects
{
    public class ForceCancelProjectCommand : IRequest<ServiceResult>
    {
        public Guid ProjectId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public ForceCancelProjectCommand(Guid projectId, Guid approverAdminId, string? reason = null)
        {
            ProjectId = projectId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class ForceCancelProjectHandler : IRequestHandler<ForceCancelProjectCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ForceCancelProjectHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ForceCancelProjectCommand command, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

            if (project == null)
                return ServiceResult.NotFound("Không tìm thấy dự án.");

            var result = project.Cancel();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(
                command.ApproverAdminId,
                "ProjectForceCancelledByAdmin",
                nameof(Project),
                project.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            foreach (var accountId in project.Members.Where(m => m.IsActive).Select(m => m.AccountId))
            {
                var notifResult = Notification.Create(
                    accountId,
                    "Dự án đã bị huỷ",
                    $"Dự án \"{project.Title}\" đã bị Admin hệ thống huỷ." +
                    (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Huỷ dự án thành công");
        }
    }
}
