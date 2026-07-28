namespace TaskMind.Applications.Admins.Dtos
{
    public class SkillDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsApproved { get; set; }

        /// <summary>Tên công ty/cơ sở đào tạo đề xuất; null nếu do Admin tự tạo.</summary>
        public string? SuggestedBy { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }

    public class SkillDetailDto : SkillDto
    {
        public List<SkillDto> RelatedSkills { get; set; } = new();
    }
}