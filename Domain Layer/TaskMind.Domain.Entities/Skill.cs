using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>Một kỹ năng trong danh mục chuẩn hoá toàn hệ thống (mục 4.15).</summary>
    public class Skill : EntityBase
    {
        public string SkillName { get; private set; } = string.Empty;
        public SkillCategory Category { get; private set; }
        public bool IsApproved { get; private set; }

        /// <summary>Tên công ty/cơ sở đào tạo đề xuất; rỗng nếu Admin tự tạo trực tiếp.</summary>
        public string SuggestedBy { get; private set; } = string.Empty;

        private readonly List<Guid> _relatedSkillIds = new();
        public IReadOnlyCollection<Guid> RelatedSkillIds => _relatedSkillIds.AsReadOnly();

        private Skill() { }

        private Skill(string name, SkillCategory category, bool isApproved, string suggestedBy)
        {
            SkillName = name;
            Category = category;
            IsApproved = isApproved;
            SuggestedBy = suggestedBy;
        }

        /// <summary>Admin tạo trực tiếp, được duyệt ngay.</summary>
        public static Result<Skill> CreateByAdmin(string name, SkillCategory category)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Skill>.Failure("Tên kỹ năng không được để trống.");
            return Result<Skill>.Success(new Skill(name.Trim(), category, isApproved: true, string.Empty));
        }

        /// <summary>Công ty/cơ sở đào tạo đề xuất kỹ năng mới, chờ Admin duyệt (mục 4.15).</summary>
        public static Result<Skill> Propose(string name, SkillCategory category, string suggestedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Skill>.Failure("Tên kỹ năng không được để trống.");
            if (string.IsNullOrWhiteSpace(suggestedBy))
                return Result<Skill>.Failure("Phải xác định nguồn đề xuất.");
            return Result<Skill>.Success(new Skill(name.Trim(), category, isApproved: false, suggestedBy.Trim()));
        }

        public Result Approve()
        {
            if (IsApproved) return Result.Failure("Kỹ năng đã được duyệt.");
            IsApproved = true;
            return Result.Success();
        }

        public Result LinkRelatedSkill(Guid relatedSkillId)
        {
            if (relatedSkillId == Id) return Result.Failure("Không thể liên kết kỹ năng với chính nó.");
            if (!_relatedSkillIds.Contains(relatedSkillId)) _relatedSkillIds.Add(relatedSkillId);
            return Result.Success();
        }
    }

    /// <summary>
    /// Aggregate Root SkillCatalog — quản trị toàn bộ danh mục kỹ năng dùng chung (mục 4.15).
    /// Chỉ Admin hệ thống có quyền thêm/sửa/xoá danh mục gốc.
    /// </summary>
    public class SkillCatalog : AggregateRoot
    {
        private readonly List<Skill> _skills = new();
        public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();

        public Result<Skill> AddByAdmin(string name, SkillCategory category)
        {
            if (_skills.Any(s => string.Equals(s.SkillName, name, StringComparison.OrdinalIgnoreCase)))
                return Result<Skill>.Failure("Kỹ năng đã tồn tại trong danh mục.");

            var result = Skill.CreateByAdmin(name, category);
            if (result.IsSuccess) _skills.Add(result.Data!);
            return result;
        }

        public Result<Skill> Propose(string name, SkillCategory category, string suggestedBy)
        {
            if (_skills.Any(s => string.Equals(s.SkillName, name, StringComparison.OrdinalIgnoreCase)))
                return Result<Skill>.Failure("Kỹ năng đã tồn tại hoặc đang chờ duyệt.");

            var result = Skill.Propose(name, category, suggestedBy);
            if (result.IsSuccess) _skills.Add(result.Data!);
            return result;
        }

        public Result Approve(Guid skillId)
        {
            var skill = _skills.FirstOrDefault(s => s.Id == skillId);
            if (skill == null) return Result.Failure("Không tìm thấy kỹ năng.");
            return skill.Approve();
        }

        public Result Reject(Guid skillId)
        {
            var skill = _skills.FirstOrDefault(s => s.Id == skillId);
            if (skill == null) return Result.Failure("Không tìm thấy kỹ năng.");
            if (skill.IsApproved) return Result.Failure("Không thể từ chối kỹ năng đã được duyệt.");
            _skills.Remove(skill);
            return Result.Success();
        }
    }
}