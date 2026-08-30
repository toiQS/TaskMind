using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class ChatSummaryDto
    {
        public Guid Id { get; set; }
        public List<Guid> MemberIds { get; set; } = new();
        public int MessageCount { get; set; }
        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAtUtc { get; set; }
    }

    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderAccountId { get; set; }
        public string Content { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
        public DateTime SentAtUtc { get; set; }
    }
}