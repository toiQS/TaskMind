// ReactivateCompanyCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog + Notification, đối xứng với SuspendCompanyCommand.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class ReactivateCompanyCommand : IRequest<ServiceResult>
    {
        public Guid CompanyId { get; }
        public Guid ApproverAdminId { get; }

        public ReactivateCompanyCommand(Guid companyId, Guid approverAdminId)
        {
            CompanyId = companyId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ReactivateCompanyHandler : IRequestHandler<ReactivateCompanyCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ReactivateCompanyHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ReactivateCompanyCommand command, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == command.CompanyId, cancellationToken);

            if (company == null)
                return ServiceResult.NotFound("Không tìm thấy công ty.");

            var result = company.Reactivate();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "CompanyReactivated", nameof(Company), company.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == company.Id, cancellationToken);

            if (adminCompany != null)
            {
                var notifResult = Notification.Create(
                    adminCompany.LinkedUserId,
                    "Công ty đã được kích hoạt lại",
                    $"Công ty \"{company.CompanyName}\" của bạn đã được Admin hệ thống kích hoạt lại và có thể hoạt động bình thường.",
                    NotificationType.Success);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Kích hoạt lại công ty thành công");
        }
    }
}
