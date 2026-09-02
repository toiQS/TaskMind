// VerifySchoolCommand.cs
// [CẬP NHẬT - fix] Tương tự VerifyCompanyCommand: tự động cấp AdminSchool cho User đã đăng ký
// (mục 7.3.1), dùng School.RequestedByUserId mới bổ sung.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class VerifySchoolCommand : IRequest<ServiceResult>
    {
        public Guid SchoolId { get; }
        public Guid ApproverAdminId { get; }

        public VerifySchoolCommand(Guid schoolId, Guid approverAdminId)
        {
            SchoolId = schoolId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class VerifySchoolHandler : IRequestHandler<VerifySchoolCommand, ServiceResult>
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

            // [MỚI - fix] Tự động cấp AdminSchool cho User đã đứng ra đăng ký (mục 7.3.1).
            var alreadyLinked = await _dbContext.AdminSchools.AsNoTracking()
                .AnyAsync(a => a.SchoolId == school.Id, cancellationToken);

            if (!alreadyLinked && school.RequestedByUserId != Guid.Empty)
            {
                var requester = await _dbContext.Users
                    .Include(u => u.Profile)
                    .Include(u => u.Security)
                    .FirstOrDefaultAsync(u => u.Id == school.RequestedByUserId, cancellationToken);

                if (requester != null)
                {
                    var adminSchoolResult = AdminSchool.CreateAdminSchool(
                        requester.Profile.CitizenId,
                        requester.Profile.Email,
                        requester.Security.PasswordHash,
                        school.Id,
                        requester.Id);

                    if (adminSchoolResult.IsSuccess)
                        _dbContext.AdminSchools.Add(adminSchoolResult.Data!);
                }
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SchoolVerified", nameof(School), school.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt cơ sở đào tạo thành công");
        }
    }
}
