// SuspendSchoolCommand.cs
// [CẬP NHẬT - fix] Tương tự SuspendCompanyCommand: thêm ApproverAdminId + AuditLog + Notification.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class SuspendSchoolCommand : IRequest<ServiceResult>
    {
        public Guid SchoolId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public SuspendSchoolCommand(Guid schoolId, Guid approverAdminId, string? reason = null)
        {
            SchoolId = schoolId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class SuspendSchoolHandler : IRequestHandler<SuspendSchoolCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public SuspendSchoolHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(SuspendSchoolCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var result = school.Suspend();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SchoolSuspended", nameof(School), school.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminSchool = await _dbContext.AdminSchools
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.SchoolId == school.Id, cancellationToken);

            if (adminSchool != null)
            {
                var notifResult = Notification.Create(
                    adminSchool.LinkedUserId,
                    "Cơ sở đào tạo đã bị tạm ngưng",
                    $"Cơ sở đào tạo \"{school.SchoolName}\" của bạn đã bị Admin hệ thống tạm ngưng hoạt động." +
                    (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Tạm ngưng cơ sở đào tạo thành công");
        }
    }
}
