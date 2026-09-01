// ChangeCompanyMembershipPackageCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class ChangeCompanyMembershipPackageCommand : IRequest<ServiceResult>
    {
        public Guid CompanyId { get; }
        public string Package { get; }

        public ChangeCompanyMembershipPackageCommand(Guid companyId, string package)
        {
            CompanyId = companyId;
            Package = package;
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

            var result = company.ChangeMembershipPackage(command.Package);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Cập nhật gói dịch vụ thành công");
        }
    }
}