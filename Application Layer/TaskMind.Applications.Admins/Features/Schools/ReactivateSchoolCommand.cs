// ReactivateSchoolCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog + Notification, đối xứng với SuspendSchoolCommand.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class ReactivateSchoolCommand : IRequest<ServiceResult>
    {
        public Guid SchoolId { get; }
        public Guid ApproverAdminId { get; }

        public ReactivateSchoolCommand(Guid schoolId, Guid approverAdminId)
        {
            SchoolId = schoolId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ReactivateSchoolHandler : IRequestHandler<ReactivateSchoolCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ReactivateSchoolHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ReactivateSchoolCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var result = school.Reactivate();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SchoolReactivated", nameof(School), school.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminSchool = await _dbContext.AdminSchools
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.SchoolId == school.Id, cancellationToken);

            if (adminSchool != null)
            {
                var notifResult = Notification.Create(
                    adminSchool.LinkedUserId,
                    "Cơ sở đào tạo đã được kích hoạt lại",
                    $"Cơ sở đào tạo \"{school.SchoolName}\" của bạn đã được Admin hệ thống kích hoạt lại và có thể hoạt động bình thường.",
                    NotificationType.Success);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Kích hoạt lại cơ sở đào tạo thành công");
        }
    }
}
