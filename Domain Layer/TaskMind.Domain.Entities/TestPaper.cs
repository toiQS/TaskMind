using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root TestPaper [MỚI] — bài kiểm tra dùng chung cho công ty và cơ sở đào tạo (mục 4.6, 4.11).</summary>
    [Index(nameof(OwnerType), nameof(OwnerId))]
    public class TestPaper : AggregateRoot
    {
        public TestOwnerType OwnerType { get; private set; }

        /// <summary>Company.Id nếu OwnerType = Company, School.Id nếu OwnerType = School.</summary>
        public Guid OwnerId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public int DurationMinutes { get; private set; }

        private TestPaper() { }

        private TestPaper(TestOwnerType ownerType, Guid ownerId, string title, int durationMinutes)
        {
            OwnerType = ownerType;
            OwnerId = ownerId;
            Title = title;
            DurationMinutes = durationMinutes;
        }

        public static Result<TestPaper> Create(TestOwnerType ownerType, Guid ownerId, string title, int durationMinutes)
        {
            if (ownerId == Guid.Empty)
                return Result<TestPaper>.Failure("OwnerId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(title))
                return Result<TestPaper>.Failure("Tiêu đề bài kiểm tra không được để trống.");
            if (durationMinutes <= 0)
                return Result<TestPaper>.Failure("Thời gian làm bài phải lớn hơn 0.");

            return Result<TestPaper>.Success(new TestPaper(ownerType, ownerId, title.Trim(), durationMinutes));
        }

        public Result UpdateDuration(int durationMinutes)
        {
            if (durationMinutes <= 0) return Result.Failure("Thời gian làm bài phải lớn hơn 0.");
            DurationMinutes = durationMinutes;
            return Result.Success();
        }
    }
}
