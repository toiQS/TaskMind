using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Chats
{
    /// <summary>
    /// Gửi ảnh trong nhóm trò chuyện (mục 4.22).
    /// TODO: Domain.Entities.Message hiện chỉ có Content (string), chưa có MessageType/AttachmentUrl
    /// riêng cho ảnh. Tạm lưu ImageUrl vào Content với tiền tố "[image]" để tầng Presentation phân biệt.
    /// Khi Domain bổ sung MessageType, cần cập nhật lại handler này.
    /// </summary>
    internal class SendImageCommand : ServiceResult<MessageDto>
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

    internal class SendImageHandler
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
                Content = command.ImageUrl, // trả về URL gốc cho UI, không kèm tiền tố nội bộ
                Status = msg.Status,
                SentAtUtc = msg.SentAtUtc
            }, "Gửi ảnh thành công");
        }
    }
}