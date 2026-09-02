// ChangeCompanyMembershipPackageCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog + Notification cho công ty (thay đổi gói dịch vụ
// ảnh hưởng trực tiếp tới quyền lợi/mức phí công ty phải trả — mục 4.4, 4.14).
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class ChangeCompanyMembershipPackageCommand : IRequest<ServiceResult>
    {
        public Guid CompanyId { get; }
        public string Package { get; }
        public Guid ApproverAdminId { get; }

        public ChangeCompanyMembershipPackageCommand(Guid companyId, string package, Guid approverAdminId)
        {
            CompanyId = companyId;
            Package = package;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ChangeCompanyMembershipPackageHandler : IRequestHandler<ChangeCompanyMembershipPackageCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeCompanyMembershipPackageHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ChangeCompanyMembershipPackageCommand command, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == command.CompanyId, cancellationToken);

            if (company == null)
                return ServiceResult.NotFound("Không tìm thấy công ty.");

            var oldPackage = company.MembershipPackage;

            var result = company.ChangeMembershipPackage(command.Package);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "CompanyMembershipPackageChanged", nameof(Company), company.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == company.Id, cancellationToken);

            if (adminCompany != null)
            {
                var notifResult = Notification.Create(
                    adminCompany.LinkedUserId,
                    "Gói dịch vụ đã được cập nhật",
                    $"Gói dịch vụ của công ty \"{company.CompanyName}\" đã được đổi từ \"{oldPackage}\" sang \"{company.MembershipPackage}\".",
                    NotificationType.System);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Cập nhật gói dịch vụ thành công");
        }
    }
}
