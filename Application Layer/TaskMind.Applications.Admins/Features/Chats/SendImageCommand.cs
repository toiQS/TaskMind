// SendImageCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Chats
{
    public class SendImageCommand : IRequest<ServiceResult<MessageDto>>
    {
        public Guid ChatId { get; }
        public Guid SenderAccountId { get; }
        public string ImageUrl { get; }

        public SendImageCommand(Guid chatId, Guid senderAccountId, string imageUrl)
        {
            ChatId = chatId;
            SenderAccountId = senderAccountId;
            ImageUrl = imageUrl;
        }
    }

    public class SendImageHandler : IRequestHandler<SendImageCommand, ServiceResult<MessageDto>>
    {
        private const string ImagePrefix = "[image]";
        private readonly IApplicationDbContext _dbContext;

        public SendImageHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<MessageDto>> Handle(SendImageCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.ImageUrl))
                return ServiceResult<MessageDto>.Failure("Đường dẫn ảnh không được để trống.");

            var chat = await _dbContext.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == command.ChatId, cancellationToken);

            if (chat == null)
                return ServiceResult<MessageDto>.NotFound("Không tìm thấy nhóm trò chuyện.");

            var messageResult = chat.SendMessage(command.SenderAccountId, $"{ImagePrefix}{command.ImageUrl}");
            if (!messageResult.IsSuccess)
                return ServiceResult<MessageDto>.Failure(messageResult.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var msg = messageResult.Data!;
            return ServiceResult<MessageDto>.Success(new MessageDto
            {
                Id = msg.Id,
                ChatId = chat.Id,
                SenderAccountId = msg.SenderAccountId,
                Content = command.ImageUrl,
                Status = msg.Status,
                SentAtUtc = msg.SentAtUtc
            }, "Gửi ảnh thành công");
        }
    }
}