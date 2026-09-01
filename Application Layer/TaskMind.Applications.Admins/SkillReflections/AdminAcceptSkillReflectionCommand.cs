// AdminAcceptSkillReflectionCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class AdminAcceptSkillReflectionCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid ApproverAdminId { get; }
        public AdminAcceptSkillReflectionCommand(Guid requestId, Guid approverAdminId)
        {
            RequestId = requestId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class AdminAcceptSkillReflectionHandler : IRequestHandler<AdminAcceptSkillReflectionCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;
        public AdminAcceptSkillReflectionHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<ServiceResult> Handle(AdminAcceptSkillReflectionCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy đề xuất phản ánh kỹ năng.");

            var result = request.AdminAccept(command.ApproverAdminId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillReflectionDownAccepted",
                nameof(CompanySkillReflectionRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Đã chấp nhận xử lý đề xuất hạ cấp, chờ nhân sự làm lại bài kiểm tra");
        }
    }
}