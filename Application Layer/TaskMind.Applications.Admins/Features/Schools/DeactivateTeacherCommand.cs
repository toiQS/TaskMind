// DeactivateTeacherCommand.cs — [MỚI - fix] tương tự DeactivateStaffCommand, cho Teacher (mục 2.1.1).
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class DeactivateTeacherCommand : IRequest<ServiceResult>
    {
        public Guid TeacherAccountId { get; }
        public Guid ApproverAdminId { get; }

        public DeactivateTeacherCommand(Guid teacherAccountId, Guid approverAdminId)
        {
            TeacherAccountId = teacherAccountId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class DeactivateTeacherHandler : IRequestHandler<DeactivateTeacherCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeactivateTeacherHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(DeactivateTeacherCommand command, CancellationToken cancellationToken)
        {
            var teacher = await _dbContext.Teachers
                .FirstOrDefaultAsync(t => t.Id == command.TeacherAccountId, cancellationToken);

            if (teacher == null)
                return ServiceResult.NotFound("Không tìm thấy giảng viên.");

            var result = teacher.Deactivate();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "TeacherLeftSchool", nameof(Teacher), teacher.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Ghi nhận giảng viên rời cơ sở đào tạo thành công");
        }
    }
}
