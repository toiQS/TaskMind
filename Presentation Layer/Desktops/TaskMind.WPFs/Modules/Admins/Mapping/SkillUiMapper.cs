using TaskMind.Applications.Admins.Dtos;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Mapping
{
    public static class SkillUiMapper
    {
        public static SkillModel ToUi(SkillDto dto) => new SkillModel
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Category = Enum.TryParse<SkillCategory>(dto.Category, true, out var cat) ? cat : SkillCategory.Other,
            // SkillDto không có Level (Level là thuộc SkillProfile record của User, không thuộc danh mục chuẩn hoá).
            // Danh mục kỹ năng chuẩn hoá (mục 4.15) không có "Level cố định" ở Domain hiện tại;
            // WPF UI đang hiển thị Level như một thuộc tính của danh mục — giữ mặc định Beginner
            // cho tới khi Domain bổ sung DifficultyLevel cho Skill catalog nếu cần.
            Level = SkillLevel.Beginner,
            IsApproved = dto.IsApproved,
            SuggestedBy = dto.SuggestedBy,
            CreatedDate = dto.CreatedDateUtc,
            UsageCount = 0
        };

        public static void ApplyDetail(DetailSkillModel model, SkillDetailDto dto)
        {
            model.Skill.UsageCount = dto.UsageCount;

            model.RelatedSkills.Clear();
            foreach (var r in dto.RelatedSkills)
                model.RelatedSkills.Add(ToUi(r));

            // TotalProjectsRequiring / TotalEndorsements / TopUsers / UsageBySource / GrowthChart / ApprovalHistory
            // chưa có Query tương ứng ở Application.Admins — giữ mock cho tới khi bổ sung.
        }
    }
}