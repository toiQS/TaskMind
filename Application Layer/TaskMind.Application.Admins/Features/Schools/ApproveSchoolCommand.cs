using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class ApproveSchoolCommand : IRequest<SchoolDto>
    {
        public Guid SchoolId { get; set; }
    }

    public class ApproveSchoolCommandHandler : IRequestHandler<ApproveSchoolCommand, SchoolDto>
    {
        private readonly IApplicationDbContext _db;

        public ApproveSchoolCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SchoolDto> Handle(ApproveSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy cơ sở đào tạo.");

            var result = school.Verify();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return SchoolMapper.ToDto(school);
        }
    }
}
