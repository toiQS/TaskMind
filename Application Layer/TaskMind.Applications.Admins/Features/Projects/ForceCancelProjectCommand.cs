using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Projects
{
    /// <summary>Admin buộc huỷ một dự án vi phạm nghiêm trọng (gian lận, khiếu nại...).</summary>
    internal class ForceCancelProjectCommand : ServiceResult
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

    internal class ForceCancelProjectHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public ForceCancelProjectHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ForceCancelProjectCommand command, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

            if (project == null)
                return ServiceResult.NotFound("Không tìm thấy dự án.");

            var result = project.Cancel();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            // TODO: AuditLog.Record(command.ApproverAdminId, "ProjectForceCancelledByAdmin", nameof(Project), project.Id, command.Reason)

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Huỷ dự án thành công");
        }
    }
}