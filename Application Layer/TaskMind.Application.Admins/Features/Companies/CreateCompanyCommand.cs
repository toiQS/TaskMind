using MediatR;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class CreateCompanyCommand : IRequest<CompanyDto>
    {
        public string Name { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        /// <summary>"Starter" | "Pro" | "Enterprise"</summary>
        public string Package { get; set; } = "Starter";
    }

    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;

        public CreateCompanyCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            var address = new Address(request.Street ?? string.Empty, request.City ?? string.Empty, request.Country ?? string.Empty);

            var result = Company.Create(request.Name, request.TaxCode, request.Field, request.Email, request.Phone ?? string.Empty, address);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            var company = result.Data!;
            var packageResult = company.ChangeMembershipPackage(request.Package);
            if (!packageResult.IsSuccess)
                throw new InvalidOperationException(packageResult.Message);

            _db.Companies.Add(company);
            await _db.SaveChangesAsync(cancellationToken);

            return CompanyMapper.ToDto(company);
        }
    }
}
