// ForceCancelProjectCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

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

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Huỷ dự án thành công");
        }
    }
}