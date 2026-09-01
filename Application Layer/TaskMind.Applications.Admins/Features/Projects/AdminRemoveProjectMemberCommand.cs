using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Projects
{
    /// <summary>Admin buộc loại một thành viên khỏi dự án (kiểm duyệt), dùng chung Project.RemoveMember() với Owner.</summary>
    public class AdminRemoveProjectMemberCommand : ServiceResult
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

    public class AdminRemoveProjectMemberHandler
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Loại thành viên khỏi dự án thành công");
        }
    }
}