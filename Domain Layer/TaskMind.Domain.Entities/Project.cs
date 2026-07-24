using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root Project, dùng chung cho dự án công ty / cơ sở đào tạo / mã nguồn mở
    /// (theo bảng DDD mục 6). OwningEntityId trỏ tới Company/School tương ứng, null nếu là OpenSource.
    /// </summary>
    public class Project : AuditableAggregateRoot
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public ProjectSourceType SourceType { get; private set; }
        public ProjectStatus ProjectStatus { get; private set; } = ProjectStatus.InProgress;

        /// <summary>Id của Company/School sở hữu dự án; null nếu SourceType = OpenSource.</summary>
        public Guid? OwningEntityId { get; private set; }

        /// <summary>true nếu là dự án trao đổi có tính chất thương mại (mục 4.14), false nếu là dự án nội bộ/OSS.</summary>
        public bool IsExchangeProject { get; private set; }

        private readonly List<ProjectMember> _members = new();
        public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

        private Project() { }

        private Project(string title, string description, ProjectSourceType sourceType, Guid? owningEntityId, bool isExchangeProject)
        {
            Title = title;
            Description = description;
            SourceType = sourceType;
            OwningEntityId = owningEntityId;
            IsExchangeProject = isExchangeProject;
        }

        /// <summary>Khởi tạo dự án; ownerAccountId sẽ tự động được gán vai trò Owner.</summary>
        public static Result<Project> Create(
            string title,
            string description,
            ProjectSourceType sourceType,
            Guid ownerAccountId,
            Guid? owningEntityId = null,
            bool isExchangeProject = false)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result<Project>.Failure("Tên dự án không được để trống.");

            if (sourceType != ProjectSourceType.OpenSource && owningEntityId is null)
                return Result<Project>.Failure("Dự án thuộc công ty/cơ sở đào tạo phải có OwningEntityId.");

            if (isExchangeProject && sourceType == ProjectSourceType.OpenSource)
                return Result<Project>.Failure("Dự án mã nguồn mở không phát sinh phí giao dịch (mục 4.12).");

            var project = new Project(title.Trim(), description?.Trim() ?? string.Empty, sourceType, owningEntityId, isExchangeProject);

            var ownerResult = ProjectMember.Create(ownerAccountId, ProjectRole.Owner);
            if (!ownerResult.IsSuccess)
                return Result<Project>.Failure(ownerResult.Message);

            project._members.Add(ownerResult.Data!);
            return Result<Project>.Success(project);
        }

        /// <summary>Gán vai trò dự án cho một thành viên (mục 4.7: thiết lập vai trò dự án).</summary>
        public Result AssignMember(Guid accountId, ProjectRole role)
        {
            if (ProjectStatus == ProjectStatus.Completed || ProjectStatus == ProjectStatus.Cancelled)
                return Result.Failure("Không thể thêm thành viên vào dự án đã kết thúc.");

            var existing = _members.FirstOrDefault(m => m.AccountId == accountId && m.IsActive);
            if (existing != null)
                return existing.ChangeRole(role);

            var result = ProjectMember.Create(accountId, role);
            if (!result.IsSuccess) return Result.Failure(result.Message);

            _members.Add(result.Data!);
            return Result.Success();
        }

        public Result RemoveMember(Guid accountId)
        {
            var member = _members.FirstOrDefault(m => m.AccountId == accountId && m.IsActive);
            if (member == null) return Result.Failure("Không tìm thấy thành viên đang hoạt động trong dự án.");
            return member.Leave();
        }

        public Result Pause()
        {
            if (ProjectStatus != ProjectStatus.InProgress) return Result.Failure("Chỉ có thể tạm dừng dự án đang thực hiện.");
            ProjectStatus = ProjectStatus.Paused;
            return Result.Success();
        }

        public Result Resume()
        {
            if (ProjectStatus != ProjectStatus.Paused) return Result.Failure("Chỉ có thể tiếp tục dự án đang tạm dừng.");
            ProjectStatus = ProjectStatus.InProgress;
            return Result.Success();
        }

        public Result Cancel()
        {
            if (ProjectStatus == ProjectStatus.Completed) return Result.Failure("Dự án đã hoàn thành, không thể huỷ.");
            ProjectStatus = ProjectStatus.Cancelled;
            return Result.Success();
        }

        /// <summary>
        /// Hoàn thành dự án và phát sinh ProjectCompletedEvent để cập nhật SkillProfile
        /// của thành viên và tạo Invoice nếu là dự án trao đổi (liên kết mục 6 - DDD note).
        /// </summary>
        public Result Complete()
        {
            if (ProjectStatus != ProjectStatus.InProgress && ProjectStatus != ProjectStatus.Paused)
                return Result.Failure("Chỉ có thể hoàn thành dự án đang thực hiện hoặc tạm dừng.");

            ProjectStatus = ProjectStatus.Completed;

            AddDomainEvent(new ProjectCompletedEvent
            {
                ProjectId = Id,
                MemberAccountIds = _members.Where(m => m.IsActive).Select(m => m.AccountId).ToArray(),
                IsExchangeProject = IsExchangeProject
            });

            return Result.Success();
        }
    }
}