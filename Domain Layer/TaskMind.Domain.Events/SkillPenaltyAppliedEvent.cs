// SkillPenaltyAppliedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>
    /// Phát sinh khi hệ thống hạ level kỹ năng do xác minh không đạt, mức phạt gấp đôi (x2) mức thông
    /// thường, ghi nhận như cảnh báo chính thức đầu tiên trên tài khoản (mục 4.3.1).
    /// </summary>
    public class SkillPenaltyAppliedEvent : DomainEvent
    {
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillLevel PreviousLevel { get; init; }
        public SkillLevel NewLevel { get; init; }
        public int PenaltyMultiplier { get; init; }
    }
}