using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class SkillLevelUpRequestListItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public SkillLevel CurrentLevel { get; set; }
        public SkillLevelUpMethod RequestType { get; set; }
        public SkillLevelUpRequestStatus RequestStatus { get; set; }
        public Guid ApproverAccountId { get; set; }
        public Guid? SubmissionId { get; set; }
    }

    public class GetSkillLevelUpRequestsFilter
    {
        public SkillLevelUpRequestStatus? RequestStatus { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ApproverAccountId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}