using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Thực thể con [MỚI] thuộc aggregate Chat (mục 4.22). Không tự phát sinh domain event —
    /// việc gửi tin được điều phối bởi Chat.SendMessage để đảm bảo bất biến (người gửi phải thuộc nhóm).
    /// </summary>
    public class Message
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ChatId { get; private set; }
        public Guid SenderAccountId { get; private set; }
        public string Content { get; private set; } = string.Empty;

        /// <summary>Active = đã gửi bình thường; Deleted = đã thu hồi.</summary>
        public EntityStatus Status { get; private set; } = EntityStatus.Active;
        public DateTime SentAtUtc { get; private set; } = DateTime.UtcNow;

        private Message() { }

        private Message(Guid chatId, Guid senderAccountId, string content)
        {
            ChatId = chatId;
            SenderAccountId = senderAccountId;
            Content = content;
        }

        public static Result<Message> Create(Guid chatId, Guid senderAccountId, string content)
        {
            if (chatId == Guid.Empty)
                return Result<Message>.Failure("ChatId không hợp lệ.");
            if (senderAccountId == Guid.Empty)
                return Result<Message>.Failure("SenderAccountId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(content))
                return Result<Message>.Failure("Nội dung tin nhắn không được để trống.");

            return Result<Message>.Success(new Message(chatId, senderAccountId, content.Trim()));
        }

        /// <summary>Thu hồi tin nhắn (mục 4.22: Message.Status "đã gửi/đã thu hồi").</summary>
        public Result Recall()
        {
            if (Status == EntityStatus.Deleted) return Result.Failure("Tin nhắn đã được thu hồi trước đó.");
            Status = EntityStatus.Deleted;
            return Result.Success();
        }
    }
}
