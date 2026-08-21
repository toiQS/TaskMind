using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Teacher là tài khoản phái sinh từ User (mục 2.1, 4.1.1), được cấp khi User được một
    /// cơ sở đào tạo (School) mời và xác minh thành công (mục 4.9). LinkedUserId trỏ về đúng
    /// tài khoản User gốc.
    /// </summary>
    [Index(nameof(SchoolId), nameof(IsActive))]
    [Index(nameof(LinkedUserId), IsUnique = true)]
    public class Teacher : Account
    {
        public Guid LinkedUserId { get; private set; }
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        private Teacher() : base() { }

        private Teacher(Guid linkedUserId, Guid schoolId)
        {
            LinkedUserId = linkedUserId;
            SchoolId = schoolId;
        }

        public static Result<Teacher> Create(
            string citizenId,
            string email,
            string passwordHash,
            Guid linkedUserId,
            Guid schoolId)
        {
            if (linkedUserId == Guid.Empty)
                return Result<Teacher>.Failure("LinkedUserId không hợp lệ.");
            if (schoolId == Guid.Empty)
                return Result<Teacher>.Failure("SchoolId không hợp lệ.");

            var teacher = new Teacher(linkedUserId, schoolId);
            var result = teacher.InitializeWithCredentials(citizenId, email, AccountRole.Teacher, passwordHash);
            if (!result.IsSuccess)
                return Result<Teacher>.Failure(result.Message);

            teacher.AddDomainEvent(new TeacherJoinedEvent
            {
                TeacherAccountId = teacher.Id,
                LinkedUserId = linkedUserId,
                SchoolId = schoolId
            });

            return Result<Teacher>.Success(teacher);
        }

        public Result Deactivate()
        {
            if (!IsActive) return Result.Failure("Giảng viên đã ở trạng thái ngừng hoạt động.");
            IsActive = false;
            return Result.Success();
        }

        public Result Reactivate()
        {
            if (IsActive) return Result.Failure("Giảng viên đang hoạt động.");
            IsActive = true;
            return Result.Success();
        }
    }
}
