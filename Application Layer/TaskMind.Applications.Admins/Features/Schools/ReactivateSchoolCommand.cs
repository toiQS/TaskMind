using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class ReactivateSchoolCommand : ServiceResult
    {
        public Guid SchoolId { get; }

        public ReactivateSchoolCommand(Guid schoolId)
        {
            SchoolId = schoolId;
        }
    }

    public class ReactivateSchoolHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public ReactivateSchoolHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ReactivateSchoolCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var result = school.Reactivate();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Kích hoạt lại cơ sở đào tạo thành công");
        }
    }
}