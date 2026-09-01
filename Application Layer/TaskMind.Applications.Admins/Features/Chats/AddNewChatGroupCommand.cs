// AddNewChatGroupCommand.cs
using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Chats
{
    public class AddNewChatGroupCommand : IRequest<ServiceResult<Guid>>
    {
        public List<Guid> MemberAccountIds { get; }

        public AddNewChatGroupCommand(List<Guid> memberAccountIds)
        {
            MemberAccountIds = memberAccountIds;
        }
    }

    public class AddNewChatGroupHandler : IRequestHandler<AddNewChatGroupCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public AddNewChatGroupHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(AddNewChatGroupCommand command, CancellationToken cancellationToken)
        {
            var chatResult = Chat.Create(command.MemberAccountIds ?? new List<Guid>());
            if (!chatResult.IsSuccess)
                return ServiceResult<Guid>.Failure(chatResult.Message);

            _dbContext.Chats.Add(chatResult.Data!);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Success(chatResult.Data!.Id, "Tạo nhóm trò chuyện thành công");
        }
    }
}