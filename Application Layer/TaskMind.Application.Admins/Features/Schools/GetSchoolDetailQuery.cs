using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class GetSchoolDetailQuery : IRequest<SchoolDetailDto>
    {
        public Guid SchoolId { get; set; }
    }

    public class GetSchoolDetailQueryHandler : IRequestHandler<GetSchoolDetailQuery, SchoolDetailDto>
    {
        private readonly IApplicationDbContext _db;

        public GetSchoolDetailQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SchoolDetailDto> Handle(GetSchoolDetailQuery request, CancellationToken cancellationToken)
        {
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy cơ sở đào tạo.");

            var teacherCount = await _db.Teachers.CountAsync(t => t.SchoolId == request.SchoolId, cancellationToken);
            var studentCount = await _db.Students.CountAsync(s => s.SchoolId == request.SchoolId, cancellationToken);
            var projectCount = await _db.Projects.CountAsync(
                p => p.OwningEntityId == request.SchoolId && p.SourceType == ProjectSourceType.School,
                cancellationToken);

            return SchoolMapper.ToDetailDto(school, teacherCount, studentCount, projectCount);
        }
    }
}
