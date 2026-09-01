// Application Layer/TaskMind.Applications.Admins/Features/SkillReflections/AdminDismissSkillReflectionCommand.cs
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    /// <summary>Admin hệ thống từ chối xử lý đề xuất hạ cấp ngay từ đầu — bằng chứng chưa thoả đáng (mục 4.3.2).</summary>
    public class AdminDismissSkillReflectionCommand : ServiceResult
    {
        public Guid RequestId { get; }
        public Guid ApproverAdminId { get; }
        public string Reason { get; }
        public AdminDismissSkillReflectionCommand(Guid requestId, Guid approverAdminId, string reason)
        {
            RequestId = requestId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class AdminDismissSkillReflectionHandler
    {
        private readonly IApplicationDbContext _dbContext;
        public AdminDismissSkillReflectionHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<ServiceResult> Handle(AdminDismissSkillReflectionCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy đề xuất phản ánh kỹ năng.");

            var result = request.AdminDismiss(command.Reason);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillReflectionDownDismissed",
                nameof(CompanySkillReflectionRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Đã từ chối đề xuất hạ cấp");
        }
    }
}