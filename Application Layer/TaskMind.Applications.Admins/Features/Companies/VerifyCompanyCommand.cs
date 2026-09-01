// VerifyCompanyCommand.cs
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

            var auditResult = AuditLog.Record(command.ApproverAdminId, "CompanyVerified", nameof(Company), company.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt công ty thành công");
        }
    }
}