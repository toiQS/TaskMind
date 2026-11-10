using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
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
        private readonly List<TestQuestion> _questions = new();
        public IReadOnlyCollection<TestQuestion> Questions => _questions.AsReadOnly();

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
            if (!Enum.IsDefined(ownerType))
                return Result<TestPaper>.Failure("Loại chủ sở hữu bài kiểm tra không hợp lệ.");
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

        public Result UpdateDetails(string title, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure("Tiêu đề bài kiểm tra không được để trống.");
            if (durationMinutes <= 0)
                return Result.Failure("Thời gian làm bài phải lớn hơn 0.");

            Title = title.Trim();
            DurationMinutes = durationMinutes;
            return Result.Success();
        }

        public Result<TestQuestion> AddQuestion(
            string prompt,
            IReadOnlyCollection<string> options,
            int correctOptionIndex,
            decimal points)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return Result<TestQuestion>.Failure("Nội dung câu hỏi không được để trống.");
            if (options is null || options.Count != 4 ||
                options.Any(string.IsNullOrWhiteSpace) ||
                options.Select(item => item.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4)
                return Result<TestQuestion>.Failure("Mỗi câu hỏi phải có đúng 4 lựa chọn khác nhau.");
            if (correctOptionIndex is < 0 or > 3)
                return Result<TestQuestion>.Failure("Đáp án đúng phải nằm trong khoảng 0 đến 3.");
            if (points <= 0 || points > 10)
                return Result<TestQuestion>.Failure("Điểm câu hỏi phải lớn hơn 0 và không vượt quá 10.");

            var question = TestQuestion.Create(prompt, options, correctOptionIndex, points);
            _questions.Add(question);
            return Result<TestQuestion>.Success(question);
        }

        public Result RemoveQuestion(Guid questionId)
        {
            var question = _questions.SingleOrDefault(item => item.Id == questionId);
            if (question is null) return Result.Failure("Không tìm thấy câu hỏi.");
            _questions.Remove(question);
            return Result.Success();
        }

        public decimal CalculateScore(IReadOnlyCollection<SubmissionAnswer> answers)
        {
            if (_questions.Count == 0) return 0m;
            var answersByQuestion = answers.ToDictionary(item => item.QuestionId);
            var totalPoints = _questions.Sum(item => item.Points);
            var earnedPoints = _questions
                .Where(question => answersByQuestion.TryGetValue(question.Id, out var answer) &&
                                   question.IsCorrect(answer.SelectedOptionIndex))
                .Sum(question => question.Points);
            return decimal.Round(earnedPoints / totalPoints * 10m, 2, MidpointRounding.AwayFromZero);
        }
    }

    public class TestQuestion
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Prompt { get; private set; } = string.Empty;
        public string Option0 { get; private set; } = string.Empty;
        public string Option1 { get; private set; } = string.Empty;
        public string Option2 { get; private set; } = string.Empty;
        public string Option3 { get; private set; } = string.Empty;
        public int CorrectOptionIndex { get; private set; }
        public decimal Points { get; private set; }

        public IReadOnlyCollection<string> Options =>
            new[] { Option0, Option1, Option2, Option3 };

        private TestQuestion() { }

        private TestQuestion(string prompt, IReadOnlyCollection<string> options, int correctOptionIndex, decimal points)
        {
            Prompt = prompt.Trim();
            Option0 = options.ElementAt(0).Trim();
            Option1 = options.ElementAt(1).Trim();
            Option2 = options.ElementAt(2).Trim();
            Option3 = options.ElementAt(3).Trim();
            CorrectOptionIndex = correctOptionIndex;
            Points = points;
        }

        internal static TestQuestion Create(
            string prompt, IReadOnlyCollection<string> options, int correctOptionIndex, decimal points) =>
            new(prompt, options, correctOptionIndex, points);

        public bool IsCorrect(int optionIndex) => CorrectOptionIndex == optionIndex;
    }
}
