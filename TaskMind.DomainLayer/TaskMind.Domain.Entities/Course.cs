using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
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

        public static Result<Course> Create(Teacher teacher, School school)
        {
            if (teacher is null || teacher.Id == Guid.Empty || !teacher.IsActive)
                return Result<Course>.Failure("Giảng viên không hợp lệ hoặc đã ngừng hoạt động.");
            if (school is null || school.Id == Guid.Empty || school.Status != EntityStatus.Active)
                return Result<Course>.Failure("Cơ sở đào tạo không hợp lệ hoặc chưa hoạt động.");
            if (teacher.SchoolId != school.Id)
                return Result<Course>.Failure("Giảng viên phải thuộc cơ sở đào tạo của khoá học.");

            return Result<Course>.Success(new Course(teacher.Id, school.Id));
        }

        public Result AssignTeacher(Teacher teacher)
        {
            if (teacher is null || teacher.Id == Guid.Empty || !teacher.IsActive)
                return Result.Failure("Giảng viên không hợp lệ hoặc đã ngừng hoạt động.");
            if (teacher.SchoolId != SchoolId)
                return Result.Failure("Giảng viên phải thuộc cơ sở đào tạo của khoá học.");

            TeacherId = teacher.Id;
            return Result.Success();
        }

        public Result AddStudent(Student student)
        {
            if (student is null || student.Id == Guid.Empty || !student.IsActive)
                return Result.Failure("Học viên không hợp lệ hoặc đã ngừng hoạt động.");
            if (student.SchoolId != SchoolId)
                return Result.Failure("Học viên phải thuộc cơ sở đào tạo của khoá học.");
            if (_studentIds.Contains(student.Id)) return Result.Failure("Học viên đã có trong khoá học.");
            _studentIds.Add(student.Id);
            return Result.Success();
        }

        public Result RemoveStudent(Guid studentAccountId)
        {
            if (!_studentIds.Remove(studentAccountId))
                return Result.Failure("Không tìm thấy học viên trong khoá học.");
            return Result.Success();
        }
    }
}
