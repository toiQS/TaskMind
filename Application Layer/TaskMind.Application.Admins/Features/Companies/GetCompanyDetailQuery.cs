using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class GetCompanyDetailQuery : IRequest<CompanyDetailDto>
    {
        public Guid CompanyId { get; set; }
    }

    public class GetCompanyDetailQueryHandler : IRequestHandler<GetCompanyDetailQuery, CompanyDetailDto>
    {
        private readonly IApplicationDbContext _db;

        public GetCompanyDetailQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CompanyDetailDto> Handle(GetCompanyDetailQuery request, CancellationToken cancellationToken)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy công ty.");

            var staffCount = await _db.Staffs.CountAsync(s => s.CompanyId == request.CompanyId, cancellationToken);
            var projectCount = await _db.Projects.CountAsync(
                p => p.OwningEntityId == request.CompanyId && p.SourceType == ProjectSourceType.Company,
                cancellationToken);

            return CompanyMapper.ToDetailDto(company, staffCount, projectCount);
        }
    }
}
