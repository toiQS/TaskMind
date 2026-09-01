// GetCompanyDetailQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class GetCompanyDetailQuery : IRequest<ServiceResult<CompanyDetailDto>>
    {
        public Guid CompanyId { get; }

        public GetCompanyDetailQuery(Guid companyId)
        {
            CompanyId = companyId;
        }
    }

    public class GetCompanyDetailHandler : IRequestHandler<GetCompanyDetailQuery, ServiceResult<CompanyDetailDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetCompanyDetailHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<CompanyDetailDto>> Handle(GetCompanyDetailQuery query, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == query.CompanyId, cancellationToken);

            if (company == null)
                return ServiceResult<CompanyDetailDto>.NotFound("Không tìm thấy công ty.");

            var activeStaffCount = await _dbContext.Staffs
                .CountAsync(s => s.CompanyId == query.CompanyId && s.IsActive, cancellationToken);

            var totalProjectCount = await _dbContext.Projects
                .CountAsync(p => p.OwningEntityId == query.CompanyId && p.SourceType == ProjectSourceType.Company, cancellationToken);

            var dto = new CompanyDetailDto
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                TaxCode = company.TaxCode,
                Field = company.Field,
                Email = company.Email,
                Phone = company.Phone,
                Address = company.Address,
                IsVerified = company.IsVerified,
                Status = company.Status,
                MembershipPackage = company.MembershipPackage,
                JoinDate = company.JoinDate,
                ActiveStaffCount = activeStaffCount,
                TotalProjectCount = totalProjectCount
            };

            return ServiceResult<CompanyDetailDto>.Success(dto, "Lấy chi tiết công ty thành công");
        }
    }
}