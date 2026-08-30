using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Chats
{
    /// <summary>Gửi tin nhắn văn bản trong một nhóm trò chuyện (mục 4.22), phát sinh MessageSentEvent qua Chat.SendMessage.</summary>
    internal class SendMessageCommand : ServiceResult<MessageDto>
    {
        public Guid ChatId { get; }
        public Guid SenderAccountId { get; }
        public string Content { get; }

        public SendMessageCommand(Guid chatId, Guid senderAccountId, string content)
        {
            ChatId = chatId;
            SenderAccountId = senderAccountId;
            Content = content;
        }
    }

    internal class SendMessageHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public SendMessageHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<MessageDto>> Handle(SendMessageCommand command, CancellationToken cancellationToken)
        {
            var chat = await _dbContext.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == command.ChatId, cancellationToken);

            if (chat == null)
                return ServiceResult<MessageDto>.NotFound("Không tìm thấy nhóm trò chuyện.");

            var messageResult = chat.SendMessage(command.SenderAccountId, command.Content);
            if (!messageResult.IsSuccess)
                return ServiceResult<MessageDto>.Failure(messageResult.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var msg = messageResult.Data!;
            return ServiceResult<MessageDto>.Success(new MessageDto
            {
                Id = msg.Id,
                ChatId = chat.Id,
                SenderAccountId = msg.SenderAccountId,
                Content = msg.Content,
                Status = msg.Status,
                SentAtUtc = msg.SentAtUtc
            }, "Gửi tin nhắn thành công");
        }
    }
}