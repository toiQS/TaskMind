using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class SkillMapper
    {
        public static SkillDto ToDto(Skill s) => new SkillDto
        {
            Id = s.Id,
            Name = s.SkillName,
            Category = s.Category.ToString(),
            IsApproved = s.IsApproved,
            SuggestedBy = string.IsNullOrWhiteSpace(s.SuggestedBy) ? null : s.SuggestedBy,
            CreatedDateUtc = s.CreateAt
        };
    }
}
