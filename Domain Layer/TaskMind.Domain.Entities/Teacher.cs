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
    ///
    /// [CẬP NHẬT - v2.1, mục 2.1.1] Áp dụng cùng nguyên tắc vòng đời như Staff: mỗi lần gia nhập là
    /// một bản ghi Teacher hoàn toàn mới; khi rời cơ sở đào tạo, bản ghi chuyển IsActive = false VĨNH
    /// VIỄN (không còn Reactivate) và được giữ lại làm dữ liệu lịch sử. LeftAtUtc cùng CreatedAtUtc
    /// xác định khoảng thời gian công tác.
    /// </summary>
    [Index(nameof(SchoolId), nameof(IsActive))]
    [Index(nameof(LinkedUserId), nameof(IsActive))]
    public class Teacher : Account
    {
        public Guid LinkedUserId { get; private set; }
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        /// <summary>Thời điểm rời cơ sở đào tạo — đóng băng vĩnh viễn bản ghi này (mục 2.1.1). [MỚI - v2.1]</summary>
        public DateTimeOffset? LeftAtUtc { get; private set; }

        private Teacher() : base() { }

        private Teacher(Guid linkedUserId, Guid schoolId)
        {
            LinkedUserId = linkedUserId;
            SchoolId = schoolId;
        }

        /// <summary>Cấp một bản ghi Teacher MỚI cho một lượt gia nhập cơ sở đào tạo (mục 2.1.1).</summary>
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

        /// <summary>Rời cơ sở đào tạo: đóng băng bản ghi VĨNH VIỄN (mục 2.1.1). KHÔNG có Reactivate().</summary>
        public Result Deactivate()
        {
            if (!IsActive) return Result.Failure("Giảng viên đã ở trạng thái ngừng hoạt động.");

            IsActive = false;
            LeftAtUtc = DateTimeOffset.UtcNow;

            AddDomainEvent(new TeacherLeftEvent
            {
                TeacherAccountId = Id,
                LinkedUserId = LinkedUserId,
                SchoolId = SchoolId,
                JoinedAtUtc = CreatedAtUtc,
                LeftAtUtc = LeftAtUtc.Value
            });

            return Result.Success();
        }
    }
}
