// RejectSkillCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog — trước đây ApproveSkillCommand có AuditLog nhưng
// RejectSkillCommand (thao tác đối xứng, cùng do Admin quyết định) lại không có gì, mất khả năng
// truy vết ai đã từ chối kỹ năng nào.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class RejectSkillCommand : IRequest<ServiceResult>
    {
        public Guid SkillId { get; }
        public Guid ApproverAdminId { get; }

        public RejectSkillCommand(Guid skillId, Guid approverAdminId)
        {
            SkillId = skillId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class RejectSkillHandler : IRequestHandler<RejectSkillCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public RejectSkillHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(RejectSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .FirstOrDefaultAsync(s => s.Id == command.SkillId, cancellationToken);

            if (skill == null)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng.");

            if (skill.IsApproved)
                return ServiceResult.Failure("Không thể từ chối kỹ năng đã được duyệt.");

            // Lưu lại SkillName trước khi xoá để ghi vào AuditLog cho dễ tra cứu (skill sẽ bị Remove).
            var skillName = skill.SkillName;

            _dbContext.Skills.Remove(skill);

            var auditResult = AuditLog.Record(command.ApproverAdminId, $"SkillRejected:{skillName}", nameof(Skill), skill.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Từ chối kỹ năng thành công");
        }
    }
}
