using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root Submission [MỚI] — kết quả làm bài của một User cho một TestPaper (mục 4.6, 4.11).</summary>
    [Index(nameof(TestPaperId), nameof(UserId))]
    public class Submission : AggregateRoot
    {
        public Guid TestPaperId { get; private set; }
        public Guid UserId { get; private set; }
        public decimal? Score { get; private set; }
        public DateTime? SubmittedAtUtc { get; private set; }
        public bool IsGraded => Score.HasValue;

        private Submission() { }

        private Submission(Guid testPaperId, Guid userId)
        {
            TestPaperId = testPaperId;
            UserId = userId;
        }

        public static Result<Submission> Create(Guid testPaperId, Guid userId)
        {
            if (testPaperId == Guid.Empty)
                return Result<Submission>.Failure("TestPaperId không hợp lệ.");
            if (userId == Guid.Empty)
                return Result<Submission>.Failure("UserId không hợp lệ.");

            return Result<Submission>.Success(new Submission(testPaperId, userId));
        }

        /// <summary>Nộp bài, đánh dấu thời điểm nộp (mục 4.6/4.11).</summary>
        public Result Submit()
        {
            if (SubmittedAtUtc.HasValue) return Result.Failure("Bài làm đã được nộp trước đó.");
            SubmittedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        /// <summary>Chấm điểm, phát sinh SubmissionGradedEvent để đồng bộ SkillProfile/SkillLevelUpRequest/Certificate (mục 7.3.4).</summary>
        public Result Grade(decimal score)
        {
            if (!SubmittedAtUtc.HasValue)
                return Result.Failure("Bài làm chưa được nộp, không thể chấm điểm.");
            if (score < 0)
                return Result.Failure("Điểm số không hợp lệ.");

            Score = score;

            AddDomainEvent(new SubmissionGradedEvent
            {
                SubmissionId = Id,
                TestPaperId = TestPaperId,
                UserId = UserId,
                Score = score
            });

            return Result.Success();
        }
    }
}
