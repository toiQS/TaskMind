using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class RejectCompanyCommand : IRequest<CompanyDto>
    {
        public Guid CompanyId { get; set; }
    }

    public class RejectCompanyCommandHandler : IRequestHandler<RejectCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;

        public RejectCompanyCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CompanyDto> Handle(RejectCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy công ty.");

            if (company.IsVerified)
                throw new InvalidOperationException("Công ty đã được xác thực, không thể từ chối.");

            // Company chưa có method Reject() riêng ở tầng Domain; dùng Suspend() (UpdateStatus -> Blocked)
            // trong khi IsVerified vẫn = false để biểu diễn trạng thái "Rejected" theo quy ước
            // VerifiableEntityStatusHelper (xem Common/VerifiableEntityStatusHelper.cs).
            var result = company.Suspend();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return CompanyMapper.ToDto(company);
        }
    }
}
