// SuspendCompanyCommand.cs
// [CẬP NHẬT - fix] Trước đây không có ApproverAdminId, không ghi AuditLog và không báo cho công ty bị
// tạm ngưng — không nhất quán với VerifyCompanyCommand (đã có AuditLog) và với các luồng khác trong
// hệ thống luôn thông báo cho bên bị ảnh hưởng trực tiếp.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class SuspendCompanyCommand : IRequest<ServiceResult>
    {
        public Guid CompanyId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public SuspendCompanyCommand(Guid companyId, Guid approverAdminId, string? reason = null)
        {
            CompanyId = companyId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class SuspendCompanyHandler : IRequestHandler<SuspendCompanyCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public SuspendCompanyHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(SuspendCompanyCommand command, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == command.CompanyId, cancellationToken);

            if (company == null)
                return ServiceResult.NotFound("Không tìm thấy công ty.");

            var result = company.Suspend();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "CompanySuspended", nameof(Company), company.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == company.Id, cancellationToken);

            if (adminCompany != null)
            {
                var notifResult = Notification.Create(
                    adminCompany.LinkedUserId,
                    "Công ty đã bị tạm ngưng",
                    $"Công ty \"{company.CompanyName}\" của bạn đã bị Admin hệ thống tạm ngưng hoạt động." +
                    (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Tạm ngưng công ty thành công");
        }
    }
}
