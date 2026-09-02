// RemoveChatGroupCommand.cs
// [CẬP NHẬT - fix] Thêm ApproverAdminId + AuditLog — xoá cả một nhóm trò chuyện là thao tác Admin phá
// huỷ dữ liệu, cùng mức độ nhạy cảm như RemoveReviewCommand (vốn đã có AuditLog), nhưng trước đây
// module Chat hoàn toàn không ghi AuditLog cho bất kỳ thao tác nào.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Chats
{
    public class RemoveChatGroupCommand : IRequest<ServiceResult>
    {
        public Guid ChatId { get; }
        public Guid ApproverAdminId { get; }

        public RemoveChatGroupCommand(Guid chatId, Guid approverAdminId)
        {
            ChatId = chatId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class RemoveChatGroupHandler : IRequestHandler<RemoveChatGroupCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public RemoveChatGroupHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(RemoveChatGroupCommand command, CancellationToken cancellationToken)
        {
            var chat = await _dbContext.Chats.FirstOrDefaultAsync(c => c.Id == command.ChatId, cancellationToken);
            if (chat == null)
                return ServiceResult.NotFound("Không tìm thấy nhóm trò chuyện.");

            _dbContext.Chats.Remove(chat);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ChatGroupRemovedByAdmin", nameof(Chat), chat.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Xoá nhóm trò chuyện thành công");
        }
    }
}
