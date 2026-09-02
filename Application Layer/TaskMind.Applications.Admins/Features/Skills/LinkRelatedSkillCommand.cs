// LinkRelatedSkillCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog cho thao tác chỉnh sửa danh mục kỹ năng chuẩn hoá
// (mục 4.16), cùng mức độ quan trọng như ApproveSkillCommand/CreateSkillByAdminCommand.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class LinkRelatedSkillCommand : IRequest<ServiceResult>
    {
        public Guid SkillId { get; }
        public Guid RelatedSkillId { get; }
        public Guid ApproverAdminId { get; }

        public LinkRelatedSkillCommand(Guid skillId, Guid relatedSkillId, Guid approverAdminId)
        {
            SkillId = skillId;
            RelatedSkillId = relatedSkillId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class LinkRelatedSkillHandler : IRequestHandler<LinkRelatedSkillCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public LinkRelatedSkillHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(LinkRelatedSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .FirstOrDefaultAsync(s => s.Id == command.SkillId, cancellationToken);

            if (skill == null)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng.");

            var relatedExists = await _dbContext.Skills
                .AnyAsync(s => s.Id == command.RelatedSkillId, cancellationToken);

            if (!relatedExists)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng liên quan.");

            var result = skill.LinkRelatedSkill(command.RelatedSkillId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillRelatedLinked", nameof(Skill), skill.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Liên kết kỹ năng thành công");
        }
    }
}
