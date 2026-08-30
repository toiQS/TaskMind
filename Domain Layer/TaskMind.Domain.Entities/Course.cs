using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root Course (mục 4.10 - Quản lý khoá học) [MỚI]. Gắn với một School và một Teacher
    /// phụ trách; tận dụng Status có sẵn từ EntityBase (UpdateStatus) cho trạng thái khoá học.
    /// </summary>
    [Index(nameof(SchoolId), nameof(TeacherId))]
    public class Course : AggregateRoot
    {
        public Guid TeacherId { get; private set; }
        public Guid SchoolId { get; private set; }
        public virtual School School { get; private set; } = default!;

        private readonly List<Guid> _studentIds = new();
        public IReadOnlyCollection<Guid> StudentIds => _studentIds.AsReadOnly();

        private Course() { }

        private Course(Guid teacherId, Guid schoolId)
        {
            TeacherId = teacherId;
            SchoolId = schoolId;
        }

        public static Result<Course> Create(Guid teacherId, Guid schoolId)
        {
            if (teacherId == Guid.Empty)
                return Result<Course>.Failure("TeacherId không hợp lệ.");
            if (schoolId == Guid.Empty)
                return Result<Course>.Failure("SchoolId không hợp lệ.");

            return Result<Course>.Success(new Course(teacherId, schoolId));
        }

        public Result AssignTeacher(Guid teacherId)
        {
            if (teacherId == Guid.Empty) return Result.Failure("TeacherId không hợp lệ.");
            TeacherId = teacherId;
            return Result.Success();
        }

        public Result AddStudent(Guid studentId)
        {
            if (studentId == Guid.Empty) return Result.Failure("StudentId không hợp lệ.");
            if (_studentIds.Contains(studentId)) return Result.Failure("Học viên đã có trong khoá học.");
            _studentIds.Add(studentId);
            return Result.Success();
        }

        public Result RemoveStudent(Guid studentId)
        {
            if (!_studentIds.Remove(studentId))
                return Result.Failure("Không tìm thấy học viên trong khoá học.");
            return Result.Success();
        }
    }
}
