using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root Chat [MỚI] — nhóm trò chuyện nội bộ (mục 4.22). Sở hữu các Message như thực thể
    /// con, giao tiếp với các Bounded Context khác qua MessageSentEvent (kích hoạt Notification).
    /// Lưu ý (mục 8 - vấn đề mở): phạm vi thành viên (Teacher, User mã nguồn mở, Admin hệ thống có
    /// tham gia hay không) và việc Chat có gắn ProjectId hay không vẫn cần làm rõ ở thiết kế chi tiết;
    /// hiện để MemberIds là danh sách AccountId chung, không ràng buộc theo loại tài khoản cụ thể.
    /// </summary>
    public class Chat : AggregateRoot
    {
        private readonly List<Guid> _memberIds = new();
        public IReadOnlyCollection<Guid> MemberIds => _memberIds.AsReadOnly();

        private readonly List<Message> _messages = new();
        public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

        private Chat() { }

        private Chat(IEnumerable<Guid> memberIds)
        {
            _memberIds.AddRange(memberIds.Distinct());
        }

        /// <summary>Khởi tạo nhóm chat với danh sách thành viên ban đầu.</summary>
        public static Result<Chat> Create(IEnumerable<Guid> memberIds)
        {
            var members = memberIds?.Distinct().ToList() ?? new List<Guid>();
            if (members.Count < 2)
                return Result<Chat>.Failure("Nhóm trò chuyện cần tối thiểu 2 thành viên.");

            return Result<Chat>.Success(new Chat(members));
        }

        public Result AddMember(Guid accountId)
        {
            if (accountId == Guid.Empty) return Result.Failure("AccountId không hợp lệ.");
            if (_memberIds.Contains(accountId)) return Result.Failure("Thành viên đã có trong nhóm trò chuyện.");
            _memberIds.Add(accountId);
            return Result.Success();
        }

        public Result RemoveMember(Guid accountId)
        {
            if (!_memberIds.Remove(accountId))
                return Result.Failure("Không tìm thấy thành viên trong nhóm trò chuyện.");
            return Result.Success();
        }

        /// <summary>Gửi tin nhắn mới trong nhóm; phát sinh MessageSentEvent để thông báo tới các thành viên còn lại (liên kết mục 4.17 - Notification).</summary>
        public Result<Message> SendMessage(Guid senderAccountId, string content)
        {
            if (!_memberIds.Contains(senderAccountId))
                return Result<Message>.Failure("Người gửi không thuộc nhóm trò chuyện.");

            var messageResult = Message.Create(Id, senderAccountId, content);
            if (!messageResult.IsSuccess)
                return messageResult;

            _messages.Add(messageResult.Data!);

            AddDomainEvent(new MessageSentEvent
            {
                MessageId = messageResult.Data!.Id,
                ChatId = Id,
                SenderAccountId = senderAccountId,
                RecipientAccountIds = _memberIds.Where(m => m != senderAccountId).ToArray()
            });

            return messageResult;
        }
    }
}
