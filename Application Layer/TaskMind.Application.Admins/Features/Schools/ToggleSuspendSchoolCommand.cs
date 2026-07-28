using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class ToggleSuspendSchoolCommand : IRequest<SchoolDto>
    {
        public Guid SchoolId { get; set; }
    }

    public class ToggleSuspendSchoolCommandHandler : IRequestHandler<ToggleSuspendSchoolCommand, SchoolDto>
    {
        private readonly IApplicationDbContext _db;

        public ToggleSuspendSchoolCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SchoolDto> Handle(ToggleSuspendSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy cơ sở đào tạo.");

            if (!school.IsVerified)
                throw new InvalidOperationException("Chỉ cơ sở đào tạo đã xác thực mới có thể tạm ngưng/kích hoạt lại.");

            var result = school.Status == EntityStatus.Blocked ? school.Reactivate() : school.Suspend();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return SchoolMapper.ToDto(school);
        }
    }
}
