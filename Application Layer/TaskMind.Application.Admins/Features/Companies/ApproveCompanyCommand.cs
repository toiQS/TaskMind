using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class ApproveCompanyCommand : IRequest<CompanyDto>
    {
        public Guid CompanyId { get; set; }
    }

    public class ApproveCompanyCommandHandler : IRequestHandler<ApproveCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;

        public ApproveCompanyCommandHandler(IApplicationDbContext db)
        {
            _db = db;   
        }

        public async Task<CompanyDto> Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy công ty.");

            var result = company.Verify();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return CompanyMapper.ToDto(company);
        }
    }
}
