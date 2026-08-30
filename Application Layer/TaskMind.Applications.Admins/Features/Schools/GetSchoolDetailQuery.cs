using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    internal class GetSchoolDetailQuery : ServiceResult<SchoolDetailDto>
    {
        public Guid SchoolId { get; }

        public GetSchoolDetailQuery(Guid schoolId)
        {
            SchoolId = schoolId;
        }
    }

    internal class GetSchoolDetailHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetSchoolDetailHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<SchoolDetailDto>> Handle(GetSchoolDetailQuery query, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == query.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult<SchoolDetailDto>.NotFound("Không tìm thấy cơ sở đào tạo.");

            var activeTeacherCount = await _dbContext.Teachers
                .CountAsync(t => t.SchoolId == query.SchoolId && t.IsActive, cancellationToken);

            var activeStudentCount = await _dbContext.Students
                .CountAsync(st => st.SchoolId == query.SchoolId && st.IsActive, cancellationToken);

            var totalProjectCount = await _dbContext.Projects
                .CountAsync(p => p.OwningEntityId == query.SchoolId && p.SourceType == ProjectSourceType.School, cancellationToken);

            var dto = new SchoolDetailDto
            {
                Id = school.Id,
                SchoolName = school.SchoolName,
                Field = school.Field,
                Email = school.Email,
                Phone = school.Phone,
                Address = school.Address,
                IsVerified = school.IsVerified,
                Status = school.Status,
                MembershipPackage = school.MembershipPackage,
                JoinDate = school.JoinDate,
                ActiveTeacherCount = activeTeacherCount,
                ActiveStudentCount = activeStudentCount,
                TotalProjectCount = totalProjectCount
            };

            return ServiceResult<SchoolDetailDto>.Success(dto, "Lấy chi tiết cơ sở đào tạo thành công");
        }
    }
}