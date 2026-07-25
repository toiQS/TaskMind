using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    public class AdminSchool : Account
    {
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;


        private AdminSchool(Guid schoolId) : base()
        {
            SchoolId = schoolId;
        }

        public static Result<AdminSchool> CreateAdminSchool(
            string citizenId,
            string email,
            string passwordHash,
            Guid schoolId)
        {
            var adminSchool = new AdminSchool(schoolId);
            var result = adminSchool.InitializeWithCredentials(citizenId, email, AccountRole.AdminSchool, passwordHash);
            if (!result.IsSuccess)
                return Result<AdminSchool>.Failure(result.Message);
            return Result<AdminSchool>.Success(adminSchool);
        }
    }
}
