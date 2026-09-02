// RemoveAllMessageInChatCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog, tương tự RemoveChatGroupCommand — thu hồi toàn bộ
// tin nhắn trong một nhóm là thao tác kiểm duyệt cần truy vết.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Chats
{
    public class RemoveAllMessageInChatCommand : IRequest<ServiceResult>
    {
        public Guid ChatId { get; }
        public Guid ApproverAdminId { get; }

        public RemoveAllMessageInChatCommand(Guid chatId, Guid approverAdminId)
        {
            ChatId = chatId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class RemoveAllMessageInChatHandler : IRequestHandler<RemoveAllMessageInChatCommand, ServiceResult>
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

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ChatMessagesRemovedByAdmin", nameof(Chat), chat.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Đã thu hồi toàn bộ tin nhắn trong nhóm trò chuyện.");
        }
    }
}
