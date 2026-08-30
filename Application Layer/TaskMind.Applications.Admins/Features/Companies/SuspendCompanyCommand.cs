using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    /// <summary>Admin tạm ngưng hoạt động một công ty (vi phạm chính sách, tranh chấp...).</summary>
    internal class SuspendCompanyCommand : ServiceResult
    {
        public Guid CompanyId { get; }
        public string? Reason { get; }

        public SuspendCompanyCommand(Guid companyId, string? reason = null)
        {
            CompanyId = companyId;
            Reason = reason;
        }
    }

    internal class SuspendCompanyHandler
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Tạm ngưng công ty thành công");
        }
    }
}