using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class ToggleSuspendCompanyCommand : IRequest<CompanyDto>
    {
        public Guid CompanyId { get; set; }
    }

    public class ToggleSuspendCompanyCommandHandler : IRequestHandler<ToggleSuspendCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;

        public ToggleSuspendCompanyCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CompanyDto> Handle(ToggleSuspendCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy công ty.");

            if (!company.IsVerified)
                throw new InvalidOperationException("Chỉ công ty đã xác thực mới có thể tạm ngưng/kích hoạt lại.");

            var result = company.Status == EntityStatus.Blocked ? company.Reactivate() : company.Suspend();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return CompanyMapper.ToDto(company);
        }
    }
}
