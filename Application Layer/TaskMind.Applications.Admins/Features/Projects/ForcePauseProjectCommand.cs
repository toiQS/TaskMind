using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

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
            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

            if (project == null)
                return ServiceResult.NotFound("Không tìm thấy dự án.");

            var result = project.Pause();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            // TODO: AuditLog.Record(command.ApproverAdminId, "ProjectForcePausedByAdmin", nameof(Project), project.Id, command.Reason)
            // TODO: gửi Notification tới Owner/thành viên dự án.

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Tạm dừng dự án thành công");
        }
    }
}