using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    /// <summary>Admin hệ thống duyệt một công ty đăng ký (mục 4.4) — kích hoạt CompanyVerifiedEvent qua Company.Verify().</summary>
    public class VerifyCompanyCommand : ServiceResult
    {
        public Guid CompanyId { get; }
        public Guid ApproverAdminId { get; }

        public VerifyCompanyCommand(Guid companyId, Guid approverAdminId)
        {
            CompanyId = companyId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class VerifyCompanyHandler
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

            // TODO: ghi AuditLog.Record(command.ApproverAdminId, "CompanyVerified", nameof(Company), company.Id)
            // khi IApplicationDbContext bổ sung DbSet<AuditLog> (mục 4.21, 7.3.1).

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt công ty thành công");
        }
    }
}