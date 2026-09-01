using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Projects
{
    /// <summary>Admin buộc tạm dừng một dự án vi phạm chính sách nền tảng (khác Owner tự Pause).</summary>
    public class ForcePauseProjectCommand : ServiceResult
    {
        public Guid ProjectId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public ForcePauseProjectCommand(Guid projectId, Guid approverAdminId, string? reason = null)
        {
            ProjectId = projectId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class ForcePauseProjectHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public ForcePauseProjectHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ForcePauseProjectCommand command, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

            if (project == null)
                return ServiceResult.NotFound("Không tìm thấy dự án.");

            var result = project.Pause();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ProjectForcePausedByAdmin", nameof(Project), project.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            foreach (var accountId in project.Members.Where(m => m.IsActive).Select(m => m.AccountId))
            {
                var notifResult = Notification.Create(
                    accountId,
                    "Dự án đã bị tạm dừng",
                    $"Dự án \"{project.Title}\" đã bị Admin hệ thống tạm dừng." +
                    (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Tạm dừng dự án thành công");
        }
    }
}