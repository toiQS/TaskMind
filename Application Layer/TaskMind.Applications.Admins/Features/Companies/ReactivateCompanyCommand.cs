// ReactivateCompanyCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class ReactivateCompanyCommand : IRequest<ServiceResult>
    {
        public Guid CompanyId { get; }

        public ReactivateCompanyCommand(Guid companyId)
        {
            CompanyId = companyId;
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Kích hoạt lại công ty thành công");
        }
    }
}