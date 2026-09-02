// DeactivateStudentCommand.cs — [MỚI - fix] tương tự DeactivateStaffCommand, cho Student (mục 2.1.1).
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class DeactivateStudentCommand : IRequest<ServiceResult>
    {
        public Guid StudentAccountId { get; }
        public Guid ApproverAdminId { get; }

        public DeactivateStudentCommand(Guid studentAccountId, Guid approverAdminId)
        {
            StudentAccountId = studentAccountId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class DeactivateStudentHandler : IRequestHandler<DeactivateStudentCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeactivateStudentHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(DeactivateStudentCommand command, CancellationToken cancellationToken)
        {
            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.Id == command.StudentAccountId, cancellationToken);

            if (student == null)
                return ServiceResult.NotFound("Không tìm thấy học viên.");

            var result = student.Deactivate();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "StudentLeftSchool", nameof(Student), student.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Ghi nhận học viên rời cơ sở đào tạo thành công");
        }
    }
}
