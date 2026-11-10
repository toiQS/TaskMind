using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
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
        private readonly List<SubmissionAnswer> _answers = new();
        public IReadOnlyCollection<SubmissionAnswer> Answers => _answers.AsReadOnly();

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

        public Result SaveAnswers(TestPaper testPaper, IReadOnlyCollection<SubmissionAnswerInput> answers)
        {
            if (SubmittedAtUtc.HasValue)
                return Result.Failure("Không thể thay đổi câu trả lời sau khi nộp bài.");
            if (testPaper is null || testPaper.Id != TestPaperId)
                return Result.Failure("Bài kiểm tra không khớp bài nộp.");
            if (testPaper.Questions.Count == 0)
                return Result.Failure("Bài kiểm tra chưa có câu hỏi trắc nghiệm.");
            if (answers is null || answers.Count != testPaper.Questions.Count ||
                answers.Select(item => item.QuestionId).Distinct().Count() != answers.Count)
                return Result.Failure("Phải trả lời mỗi câu hỏi đúng một lần.");

            var questionOptions = testPaper.Questions.ToDictionary(item => item.Id, item => item.Options.Count);
            if (answers.Any(answer =>
                    !questionOptions.TryGetValue(answer.QuestionId, out var optionCount) ||
                    answer.SelectedOptionIndex < 0 ||
                    answer.SelectedOptionIndex >= optionCount))
                return Result.Failure("Câu trả lời chứa câu hỏi hoặc lựa chọn không hợp lệ.");

            _answers.Clear();
            _answers.AddRange(answers.Select(item =>
                SubmissionAnswer.Create(item.QuestionId, item.SelectedOptionIndex)));
            return Result.Success();
        }

        public Result Submit(TestPaper testPaper)
        {
            if (!SubmittedAtUtc.HasValue && testPaper is not null && testPaper.Id == TestPaperId &&
                testPaper.Questions.Count > 0)
            {
                if (_answers.Count != testPaper.Questions.Count)
                    return Result.Failure("Phải trả lời tất cả câu hỏi trước khi nộp bài.");
                SubmittedAtUtc = DateTime.UtcNow;
                Score = testPaper.CalculateScore(_answers);
                AddGradedEvent();
                return Result.Success();
            }
            return Submit();
        }

        /// <summary>Chấm điểm, phát sinh SubmissionGradedEvent để đồng bộ SkillProfile/SkillLevelUpRequest/Certificate (mục 7.3.4).</summary>
        public Result Grade(decimal score)
        {
            if (!SubmittedAtUtc.HasValue)
                return Result.Failure("Bài làm chưa được nộp, không thể chấm điểm.");
            if (IsGraded)
                return Result.Failure("Bài làm đã được chấm điểm trước đó.");
            if (score < 0)
                return Result.Failure("Điểm số không hợp lệ.");

            Score = score;
            AddGradedEvent();

            return Result.Success();
        }

        private void AddGradedEvent() =>
            AddDomainEvent(new SubmissionGradedEvent
            {
                SubmissionId = Id,
                TestPaperId = TestPaperId,
                UserId = UserId,
                Score = Score!.Value
            });
    }

    public sealed record SubmissionAnswerInput(Guid QuestionId, int SelectedOptionIndex);

    public class SubmissionAnswer
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid QuestionId { get; private set; }
        public int SelectedOptionIndex { get; private set; }

        private SubmissionAnswer() { }

        private SubmissionAnswer(Guid questionId, int selectedOptionIndex)
        {
            QuestionId = questionId;
            SelectedOptionIndex = selectedOptionIndex;
        }

        internal static SubmissionAnswer Create(Guid questionId, int selectedOptionIndex) =>
            new(questionId, selectedOptionIndex);
    }
}
