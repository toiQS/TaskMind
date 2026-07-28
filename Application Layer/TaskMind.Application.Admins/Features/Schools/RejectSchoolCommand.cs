using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class RejectSchoolCommand : IRequest<SchoolDto>
    {
        public Guid SchoolId { get; set; }
    }

    public class RejectSchoolCommandHandler : IRequestHandler<RejectSchoolCommand, SchoolDto>
    {
        private readonly IApplicationDbContext _db;

        public RejectSchoolCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SchoolDto> Handle(RejectSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy cơ sở đào tạo.");

            if (school.IsVerified)
                throw new InvalidOperationException("Cơ sở đào tạo đã được xác thực, không thể từ chối.");

            // School chưa có method Reject() riêng ở tầng Domain; dùng Suspend() (UpdateStatus -> Blocked)
            // trong khi IsVerified vẫn = false để biểu diễn trạng thái "Rejected" (xem VerifiableEntityStatusHelper).
            var result = school.Suspend();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return SchoolMapper.ToDto(school);
        }
    }
}
