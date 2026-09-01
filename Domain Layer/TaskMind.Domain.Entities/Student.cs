using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Student là tài khoản phái sinh từ User (mục 2.1, 4.1.1), được cấp khi User được một
    /// School (cơ sở đào tạo) mời/ghi danh và xác minh thành công. LinkedUserId trỏ về đúng tài khoản
    /// User gốc để truy xuất thông tin cơ bản, kỹ năng và lịch sử tham gia dự án/khoá học.
    ///
    /// [CẬP NHẬT - v2.1, mục 2.1.1] Áp dụng cùng nguyên tắc vòng đời như Staff/Teacher: mỗi lần ghi
    /// danh là một bản ghi Student hoàn toàn mới; khi rời cơ sở đào tạo, bản ghi chuyển IsActive =
    /// false VĨNH VIỄN (không còn Reactivate) và được giữ lại làm dữ liệu lịch sử. LeftAtUtc cùng
    /// CreatedAtUtc xác định khoảng thời gian theo học/công tác.
    /// </summary>
    [Index(nameof(SchoolId), nameof(IsActive))]
    [Index(nameof(LinkedUserId), nameof(IsActive))]
    public class Student : Account
    {
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;

        public Guid LinkedUserId { get; private set; }
        public bool IsActive { get; private set; } = true;

        /// <summary>Thời điểm rời cơ sở đào tạo — đóng băng vĩnh viễn bản ghi này (mục 2.1.1). [MỚI - v2.1]</summary>
        public DateTimeOffset? LeftAtUtc { get; private set; }

        private Student() : base() { }

        private Student(Guid linkedUserId, Guid schoolId)
        {
            LinkedUserId = linkedUserId;
            SchoolId = schoolId;
        }

        /// <summary>Cấp một bản ghi Student MỚI cho một lượt ghi danh (mục 2.1.1).</summary>
        public static Result<Student> Create(
            string citizenId,
            string email,
            string passwordHash,
            Guid linkedUserId,
            Guid schoolId)
        {
            if (linkedUserId == Guid.Empty)
                return Result<Student>.Failure("LinkedUserId không hợp lệ.");
            if (schoolId == Guid.Empty)
                return Result<Student>.Failure("SchoolId không hợp lệ.");

            var student = new Student(linkedUserId, schoolId);
            var result = student.InitializeWithCredentials(citizenId, email, AccountRole.Student, passwordHash);
            if (!result.IsSuccess)
                return Result<Student>.Failure(result.Message);

            student.AddDomainEvent(new StudentEnrolledEvent
            {
                StudentAccountId = student.Id,
                LinkedUserId = linkedUserId,
                SchoolId = schoolId
            });

            return Result<Student>.Success(student);
        }

        /// <summary>Học viên rời cơ sở đào tạo: đóng băng bản ghi VĨNH VIỄN (mục 2.1.1). KHÔNG có Reactivate().</summary>
        public Result Deactivate()
        {
            if (!IsActive) return Result.Failure("Học viên đã ở trạng thái ngừng hoạt động.");

            IsActive = false;
            LeftAtUtc = DateTimeOffset.UtcNow;

            AddDomainEvent(new StudentLeftEvent
            {
                StudentAccountId = Id,
                LinkedUserId = LinkedUserId,
                SchoolId = SchoolId,
                JoinedAtUtc = CreatedAtUtc,
                LeftAtUtc = LeftAtUtc.Value
            });

            return Result.Success();
        }

        /// <summary>Chuyển học viên sang một cơ sở đào tạo khác (nếu nghiệp vụ cho phép chuyển trường trong khi vẫn Active).</summary>
        public Result TransferTo(Guid newSchoolId)
        {
            if (newSchoolId == Guid.Empty)
                return Result.Failure("SchoolId mới không hợp lệ.");
            if (newSchoolId == SchoolId)
                return Result.Failure("Học viên đã thuộc cơ sở đào tạo này.");

            var oldSchoolId = SchoolId;
            SchoolId = newSchoolId;

            AddDomainEvent(new StudentTransferredEvent
            {
                StudentAccountId = Id,
                LinkedUserId = LinkedUserId,
                OldSchoolId = oldSchoolId,
                NewSchoolId = newSchoolId
            });

            return Result.Success();
        }
    }
}
