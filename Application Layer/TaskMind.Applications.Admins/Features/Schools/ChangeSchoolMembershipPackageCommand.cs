// ChangeSchoolMembershipPackageCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog + Notification, tương tự ChangeCompanyMembershipPackageCommand.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class ChangeSchoolMembershipPackageCommand : IRequest<ServiceResult>
    {
        public Guid SchoolId { get; }
        public string Package { get; }
        public Guid ApproverAdminId { get; }

        public ChangeSchoolMembershipPackageCommand(Guid schoolId, string package, Guid approverAdminId)
        {
            SchoolId = schoolId;
            Package = package;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ChangeSchoolMembershipPackageHandler : IRequestHandler<ChangeSchoolMembershipPackageCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeSchoolMembershipPackageHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ChangeSchoolMembershipPackageCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var oldPackage = school.MembershipPackage;

            var result = school.ChangeMembershipPackage(command.Package);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SchoolMembershipPackageChanged", nameof(School), school.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminSchool = await _dbContext.AdminSchools
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.SchoolId == school.Id, cancellationToken);

            if (adminSchool != null)
            {
                var notifResult = Notification.Create(
                    adminSchool.LinkedUserId,
                    "Gói dịch vụ đã được cập nhật",
                    $"Gói dịch vụ của cơ sở đào tạo \"{school.SchoolName}\" đã được đổi từ \"{oldPackage}\" sang \"{school.MembershipPackage}\".",
                    NotificationType.System);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Cập nhật gói dịch vụ thành công");
        }
    }
}
