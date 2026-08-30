using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class SkillListItemDto
    {
        public Guid Id { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public SkillCategory Category { get; set; }
        public bool IsApproved { get; set; }
        public string SuggestedBy { get; set; } = string.Empty;
        public List<Guid> RelatedSkillIds { get; set; } = new();
    }

    public class GetSkillsFilter
    {
        /// <summary>null = tất cả; true = đã duyệt; false = đang chờ duyệt (mục 4.16).</summary>
        public bool? IsApproved { get; set; }
        public SkillCategory? Category { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}