// GetChatsQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Chats
{
    public class GetChatsQuery : IRequest<ServiceResult<List<ChatSummaryDto>>>
    {
        public Guid? MemberAccountId { get; }

        public GetChatsQuery(Guid? memberAccountId = null)
        {
            MemberAccountId = memberAccountId;
        }
    }

    public class GetChatsHandler : IRequestHandler<GetChatsQuery, ServiceResult<List<ChatSummaryDto>>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetChatsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<List<ChatSummaryDto>>> Handle(GetChatsQuery query, CancellationToken cancellationToken)
        {
            var chats = await _dbContext.Chats
                .AsNoTracking()
                .Include(c => c.Messages)
                .ToListAsync(cancellationToken);

            if (query.MemberAccountId.HasValue)
                chats = chats.Where(c => c.MemberIds.Contains(query.MemberAccountId.Value)).ToList();

            var result = chats
                .Select(c =>
                {
                    var lastMessage = c.Messages
                        .Where(m => m.Status != EntityStatus.Deleted)
                        .OrderByDescending(m => m.SentAtUtc)
                        .FirstOrDefault();

                    return new ChatSummaryDto
                    {
                        Id = c.Id,
                        MemberIds = c.MemberIds.ToList(),
                        MessageCount = c.Messages.Count(m => m.Status != EntityStatus.Deleted),
                        LastMessagePreview = lastMessage?.Content,
                        LastMessageAtUtc = lastMessage?.SentAtUtc
                    };
                })
                .OrderByDescending(c => c.LastMessageAtUtc)
                .ToList();

            return ServiceResult<List<ChatSummaryDto>>.Success(result, "Lấy danh sách nhóm trò chuyện thành công");
        }
    }
}