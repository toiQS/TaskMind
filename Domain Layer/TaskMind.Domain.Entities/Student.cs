using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Student là tài khoản phái sinh từ User (mục 2.1, 4.1.1), được cấp khi User được một
    /// School (cơ sở đào tạo) mời/ghi danh và xác minh thành công. Khác với AccountRole.Student
    /// (vai trò chung chưa gắn cơ sở nào), thực thể này gắn trực tiếp với một School cụ thể qua
    /// SchoolId, tương tự cách Staff gắn với Company và Teacher gắn với School (giảng dạy).
    /// LinkedUserId trỏ về đúng tài khoản User gốc để truy xuất thông tin cơ bản, kỹ năng và
    /// lịch sử tham gia dự án/khoá học.
    /// </summary>
    [Index(nameof(SchoolId), nameof(IsActive))]
    [Index(nameof(LinkedUserId), IsUnique = true)]
    public class Student : Account
    {
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;

        public Guid LinkedUserId { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Student() : base() { }

        private Student(Guid linkedUserId, Guid schoolId)
        {
            LinkedUserId = linkedUserId;
            SchoolId = schoolId;
        }

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

        /// <summary>Học viên tạm ngừng học (bảo lưu) hoặc rời cơ sở đào tạo.</summary>
        public Result Deactivate()
        {
            if (!IsActive) return Result.Failure("Học viên đã ở trạng thái ngừng hoạt động.");
            IsActive = false;
            return Result.Success();
        }

        /// <summary>Kích hoạt lại (khi học viên quay lại học sau bảo lưu).</summary>
        public Result Reactivate()
        {
            if (IsActive) return Result.Failure("Học viên đang hoạt động.");
            IsActive = true;
            return Result.Success();
        }

        /// <summary>Chuyển học viên sang một cơ sở đào tạo khác (nếu nghiệp vụ cho phép chuyển trường).</summary>
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
