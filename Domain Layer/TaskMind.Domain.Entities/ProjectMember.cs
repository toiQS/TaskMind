using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Thành viên trong một dự án, gắn AccountId với ProjectRole tương ứng.
    /// Một Account có thể giữ vai trò khác nhau ở các dự án khác nhau (mục 3).
    /// </summary>
    public class ProjectMember
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid AccountId { get; private set; }
        public ProjectRole Role { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DateTime? LeftAt { get; private set; }

        public bool IsActive => LeftAt == null;

        private ProjectMember() { }

        private ProjectMember(Guid accountId, ProjectRole role)
        {
            AccountId = accountId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
        }

        internal static Result<ProjectMember> Create(Guid accountId, ProjectRole role)
        {
            if (accountId == Guid.Empty)
                return Result<ProjectMember>.Failure("AccountId không hợp lệ.");
            return Result<ProjectMember>.Success(new ProjectMember(accountId, role));
        }

        internal Result ChangeRole(ProjectRole newRole)
        {
            if (!IsActive) return Result.Failure("Không thể đổi vai trò của thành viên đã rời dự án.");
            Role = newRole;
            return Result.Success();
        }

        internal Result Leave()
        {
            if (!IsActive) return Result.Failure("Thành viên đã rời dự án trước đó.");
            LeftAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}