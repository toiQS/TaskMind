// ApproveSkillCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class ApproveSkillCommand : IRequest<ServiceResult>
    {
        public Guid SkillId { get; }
        public Guid ApproverAdminId { get; }

        public ApproveSkillCommand(Guid skillId, Guid approverAdminId)
        {
            SkillId = skillId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ApproveSkillHandler : IRequestHandler<ApproveSkillCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ApproveSkillHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ApproveSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .FirstOrDefaultAsync(s => s.Id == command.SkillId, cancellationToken);

            if (skill == null)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng.");

            var result = skill.Approve();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillApproved", nameof(Skill), skill.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt kỹ năng thành công");
        }
    }
}