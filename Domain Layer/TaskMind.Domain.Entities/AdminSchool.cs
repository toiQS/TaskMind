using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    [Index(nameof(SchoolId), nameof(LinkedUserId), IsUnique = true)]
    public class AdminSchool : Account
    {
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;

        /// <summary>Trỏ về tài khoản User gốc đã đăng ký thành lập cơ sở đào tạo (mục 2.1, 4.1.1).</summary>
        public Guid LinkedUserId { get; private set; }

        private AdminSchool(Guid schoolId, Guid linkedUserId) : base()
        {
            SchoolId = schoolId;
            LinkedUserId = linkedUserId;
        }

        public static Result<AdminSchool> CreateAdminSchool(
            string citizenId,
            string email,
            string passwordHash,
            Guid schoolId,
            Guid linkedUserId)
        {
            var adminSchool = new AdminSchool(schoolId, linkedUserId);
            var result = adminSchool.InitializeWithCredentials(citizenId, email, AccountRole.AdminSchool, passwordHash);
            if (!result.IsSuccess)
                return Result<AdminSchool>.Failure(result.Message);

            adminSchool.AddDomainEvent(new AdminSchoolLinkedEvent
            {
                AdminSchoolAccountId = adminSchool.Id,
                LinkedUserId = linkedUserId,
                SchoolId = schoolId
            });

            return Result<AdminSchool>.Success(adminSchool);
        }
    }
}
