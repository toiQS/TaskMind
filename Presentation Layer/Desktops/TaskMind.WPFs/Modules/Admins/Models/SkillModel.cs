namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum SkillCategory
    {
        ProgrammingLanguage,
        Framework,
        SoftSkill,
        Tool,
        Other
    }

    public enum SkillLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4
    }

    public class SkillModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SkillCategory Category { get; set; }
        public SkillLevel Level { get; set; }

        /// <summary>true = đã duyệt vào danh mục chính thức, false = đề xuất đang chờ Admin duyệt</summary>
        public bool IsApproved { get; set; } = true;

        /// <summary>Tên công ty/cơ sở đào tạo đề xuất; null/rỗng nếu do Admin tự tạo</summary>
        public string SuggestedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        /// <summary>Số người dùng đã khai báo kỹ năng này trong hồ sơ cá nhân (mục 4.3).</summary>
        public int UsageCount { get; set; }
    }
}