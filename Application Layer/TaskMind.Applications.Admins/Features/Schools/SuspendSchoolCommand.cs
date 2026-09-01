// SuspendSchoolCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class SuspendSchoolCommand : IRequest<ServiceResult>
    {
        public Guid SchoolId { get; }

        public SuspendSchoolCommand(Guid schoolId)
        {
            SchoolId = schoolId;
        }
    }

    public class SuspendSchoolHandler : IRequestHandler<SuspendSchoolCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public SuspendSchoolHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(SuspendSchoolCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var result = school.Suspend();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Tạm ngưng cơ sở đào tạo thành công");
        }
    }
}