// VerifyCompanyCommand.cs
// [CẬP NHẬT - fix] Trước đây handler này chỉ gọi company.Verify() rồi ghi AuditLog — không hề tạo
// tài khoản AdminCompany dù domain đã có sẵn AdminCompany.CreateAdminCompany(). Với
// Company.RequestedByUserId mới bổ sung, giờ có thể tự động cấp AdminCompany cho đúng User đã đăng ký
// (mục 7.3.1), tái sử dụng CitizenId/Email/PasswordHash sẵn có của User đó.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class VerifyCompanyCommand : IRequest<ServiceResult>
    {
        public Guid CompanyId { get; }
        public Guid ApproverAdminId { get; }

        public VerifyCompanyCommand(Guid companyId, Guid approverAdminId)
        {
            CompanyId = companyId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class VerifyCompanyHandler : IRequestHandler<VerifyCompanyCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public VerifyCompanyHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(VerifyCompanyCommand command, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == command.CompanyId, cancellationToken);

            if (company == null)
                return ServiceResult.NotFound("Không tìm thấy công ty.");

            var result = company.Verify();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            // [MỚI - fix] Tự động cấp AdminCompany cho User đã đứng ra đăng ký (mục 7.3.1),
            // nếu chưa có AdminCompany nào liên kết với công ty này.
            var alreadyLinked = await _dbContext.AdminCompanies.AsNoTracking()
                .AnyAsync(ac => ac.CompanyId == company.Id, cancellationToken);

            if (!alreadyLinked && company.RequestedByUserId != Guid.Empty)
            {
                var requester = await _dbContext.Users
                    .Include(u => u.Profile)
                    .Include(u => u.Security)
                    .FirstOrDefaultAsync(u => u.Id == company.RequestedByUserId, cancellationToken);

                if (requester != null)
                {
                    var adminCompanyResult = AdminCompany.CreateAdminCompany(
                        requester.Profile.CitizenId,
                        requester.Profile.Email,
                        requester.Security.PasswordHash,
                        company.Id,
                        requester.Id);

                    if (adminCompanyResult.IsSuccess)
                        _dbContext.AdminCompanies.Add(adminCompanyResult.Data!);
                }
                // Nếu không tìm thấy requester (dữ liệu bất thường), vẫn cho phép Verify() thành công
                // nhưng cần Admin xử lý thủ công việc cấp AdminCompany — nên xem xét trả về cảnh báo
                // ở tầng UI thay vì chặn toàn bộ giao dịch.
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "CompanyVerified", nameof(Company), company.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt công ty thành công");
        }
    }
}
