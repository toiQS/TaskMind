using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Chats
{
    /// <summary>Admin thu hồi toàn bộ tin nhắn trong một nhóm trò chuyện (kiểm duyệt) — dùng Message.Recall() để giữ lịch sử thay vì xoá cứng.</summary>
    public class RemoveAllMessageInChatCommand : ServiceResult
    {
        public Guid ChatId { get; }

        public RemoveAllMessageInChatCommand(Guid chatId)
        {
            ChatId = chatId;
        }
    }

    public class RemoveAllMessageInChatHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public RemoveAllMessageInChatHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(RemoveAllMessageInChatCommand command, CancellationToken cancellationToken)
        {
            var chat = await _dbContext.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == command.ChatId, cancellationToken);

            if (chat == null)
                return ServiceResult.NotFound("Không tìm thấy nhóm trò chuyện.");

            foreach (var message in chat.Messages.Where(m => m.Status != EntityStatus.Deleted))
            {
                message.Recall();
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Đã thu hồi toàn bộ tin nhắn trong nhóm trò chuyện.");
        }
    }
}