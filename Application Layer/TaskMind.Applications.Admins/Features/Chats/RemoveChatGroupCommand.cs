using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Chats
{
    /// <summary>Admin xoá một nhóm trò chuyện (mục 4.22) - dùng cho mục đích kiểm duyệt/dọn dẹp.</summary>
    public class RemoveChatGroupCommand : ServiceResult
    {
        public Guid ChatId { get; }

        public RemoveChatGroupCommand(Guid chatId)
        {
            ChatId = chatId;
        }
    }

    public class RemoveChatGroupHandler
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
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Xoá nhóm trò chuyện thành công");
        }
    }
}