using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root JobPosting [MỚI] — tin tuyển dụng do Company đăng (mục 4.18).</summary>
    [Index(nameof(CompanyId), nameof(PostingStatus))]
    public class JobPosting : AggregateRoot
    {
        public Guid CompanyId { get; private set; }
        public string Title { get; private set; } = string.Empty;

        private readonly List<Guid> _requiredSkillIds = new();
        public IReadOnlyCollection<Guid> RequiredSkillIds => _requiredSkillIds.AsReadOnly();

        /// <summary>Đặt tên khác "Status" để không che khuất EntityBase.Status (EntityStatus).</summary>
        public JobPostingStatus PostingStatus { get; private set; } = JobPostingStatus.Draft;

        private JobPosting() { }

        private JobPosting(Guid companyId, string title)
        {
            CompanyId = companyId;
            Title = title;
        }

        public static Result<JobPosting> Create(Guid companyId, string title, IEnumerable<Guid>? requiredSkillIds = null)
        {
            if (companyId == Guid.Empty)
                return Result<JobPosting>.Failure("CompanyId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(title))
                return Result<JobPosting>.Failure("Tiêu đề tin tuyển dụng không được để trống.");

            var posting = new JobPosting(companyId, title.Trim());
            if (requiredSkillIds != null)
                posting._requiredSkillIds.AddRange(requiredSkillIds.Distinct());

            return Result<JobPosting>.Success(posting);
        }

        public Result AddRequiredSkill(Guid skillId)
        {
            if (skillId == Guid.Empty) return Result.Failure("SkillId không hợp lệ.");
            if (!_requiredSkillIds.Contains(skillId)) _requiredSkillIds.Add(skillId);
            return Result.Success();
        }

        public Result Open()
        {
            if (PostingStatus == JobPostingStatus.Cancelled)
                return Result.Failure("Tin tuyển dụng đã bị huỷ.");
            PostingStatus = JobPostingStatus.Open;
            return Result.Success();
        }

        public Result Close()
        {
            if (PostingStatus != JobPostingStatus.Open)
                return Result.Failure("Chỉ có thể đóng tin đang mở.");
            PostingStatus = JobPostingStatus.Closed;
            return Result.Success();
        }

        public Result Cancel()
        {
            if (PostingStatus == JobPostingStatus.Closed)
                return Result.Failure("Tin đã đóng, không thể huỷ.");
            PostingStatus = JobPostingStatus.Cancelled;
            return Result.Success();
        }
    }
}
