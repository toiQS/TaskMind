using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class ChangeSchoolMembershipPackageCommand : ServiceResult
    {
        public Guid SchoolId { get; }
        public string Package { get; }

        public ChangeSchoolMembershipPackageCommand(Guid schoolId, string package)
        {
            SchoolId = schoolId;
            Package = package;
        }
    }

    public class ChangeSchoolMembershipPackageHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeSchoolMembershipPackageHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ChangeSchoolMembershipPackageCommand command, CancellationToken cancellationToken)
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.Id == command.SchoolId, cancellationToken);

            if (school == null)
                return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");

            var result = school.ChangeMembershipPackage(command.Package);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Cập nhật gói dịch vụ thành công");
        }
    }
}