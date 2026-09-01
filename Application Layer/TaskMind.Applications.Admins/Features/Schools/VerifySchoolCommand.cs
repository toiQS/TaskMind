using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Schools
{
    /// <summary>Admin hệ thống duyệt một cơ sở đào tạo đăng ký (mục 4.8) — kích hoạt SchoolVerifiedEvent.</summary>
    public class VerifySchoolCommand : ServiceResult
    {
        public Guid SchoolId { get; }
        public Guid ApproverAdminId { get; }

        public VerifySchoolCommand(Guid schoolId, Guid approverAdminId)
        {
            SchoolId = schoolId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class VerifySchoolHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public VerifySchoolHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(VerifySchoolCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var result = school.Verify();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SchoolVerified", nameof(School), school.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt cơ sở đào tạo thành công");
        }
    }
}