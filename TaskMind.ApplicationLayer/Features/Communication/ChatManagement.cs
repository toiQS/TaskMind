using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Communication;

public sealed record CreateChatGroupRequest(IReadOnlyCollection<Guid> MemberAccountIds);
public sealed record AddChatMembersRequest(IReadOnlyCollection<Guid> MemberAccountIds);
public sealed record SendChatMessageRequest(string Content);
public sealed record ChatGroupDto(Guid Id, IReadOnlyCollection<Guid> MemberAccountIds, DateTime CreatedAtUtc);
public sealed record ChatMessageDto(
    Guid Id,
    Guid SenderAccountId,
    string? Content,
    string Status,
    DateTime SentAtUtc);
public sealed record ChatGroupPageDto(
    IReadOnlyCollection<ChatGroupDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
public sealed record ChatMessagesPageDto(IReadOnlyCollection<ChatMessageDto> Items, int PageSize);

public sealed record ListChatGroupsQuery(int Page, int PageSize)
    : IRequest<ServiceResult<ChatGroupPageDto>>;
public sealed record CreateChatGroupCommand(IReadOnlyCollection<Guid> MemberAccountIds)
    : IRequest<ServiceResult<Guid>>;
public sealed record GetChatGroupQuery(Guid GroupId) : IRequest<ServiceResult<ChatGroupDto>>;
public sealed record AddChatMembersCommand(Guid GroupId, IReadOnlyCollection<Guid> MemberAccountIds)
    : IRequest<ServiceResult>;
public sealed record SendChatMessageCommand(Guid GroupId, string Content)
    : IRequest<ServiceResult<Guid>>;
public sealed record ListChatMessagesQuery(Guid GroupId, DateTimeOffset? Before, int PageSize)
    : IRequest<ServiceResult<ChatMessagesPageDto>>;
public sealed record WithdrawChatMessageCommand(Guid GroupId, Guid MessageId)
    : IRequest<ServiceResult>;
public sealed record DeleteChatGroupCommand(Guid GroupId) : IRequest<ServiceResult>;
public sealed record WithdrawAllChatMessagesCommand(Guid GroupId) : IRequest<ServiceResult>;
public sealed record ListAuditLogsQuery(
    Guid? ActorId,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize) : IRequest<ServiceResult<AuditLogPageDto>>;
public sealed record AuditLogItemDto(
    Guid Id,
    Guid ActorAccountId,
    string Action,
    string EntityType,
    Guid EntityId,
    DateTime CreatedAtUtc);
public sealed record AuditLogPageDto(
    IReadOnlyCollection<AuditLogItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class ListChatGroupsHandler : IRequestHandler<ListChatGroupsQuery, ServiceResult<ChatGroupPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListChatGroupsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ChatGroupPageDto>> Handle(ListChatGroupsQuery query, CancellationToken ct)
    {
        if (!ChatRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<ChatGroupPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult<ChatGroupPageDto>.Forbidden();
        var chats = _db.Chats.AsNoTracking().Where(chat =>
            chat.Status == EntityStatus.Active && chat.MemberIds.Contains(account.Id));
        var count = await chats.CountAsync(ct);
        var rows = await chats.OrderByDescending(chat => chat.CreateAt).ThenBy(chat => chat.Id)
            .Skip(skip).Take(query.PageSize)
            .ToListAsync(ct);
        var items = rows.Select(chat =>
            new ChatGroupDto(chat.Id, chat.MemberIds.ToArray(), chat.CreateAt)).ToArray();
        return ServiceResult<ChatGroupPageDto>.Success(
            new ChatGroupPageDto(items, query.Page, query.PageSize, count));
    }
}

public sealed class CreateChatGroupHandler : IRequestHandler<CreateChatGroupCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CreateChatGroupHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateChatGroupCommand command, CancellationToken ct)
    {
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult<Guid>.Forbidden();
        var memberIds = (command.MemberAccountIds ?? Array.Empty<Guid>())
            .Append(account.Id).Distinct().ToArray();
        if (!await ChatRules.AreActiveAccounts(_db, memberIds, ct))
            return ServiceResult<Guid>.Failure("Một hoặc nhiều thành viên không tồn tại hoặc không hoạt động.");
        var created = Chat.Create(memberIds);
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        _db.Chats.Add(created.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã tạo nhóm trò chuyện.");
    }
}

public sealed class GetChatGroupHandler : IRequestHandler<GetChatGroupQuery, ServiceResult<ChatGroupDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public GetChatGroupHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ChatGroupDto>> Handle(GetChatGroupQuery query, CancellationToken ct)
    {
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult<ChatGroupDto>.Forbidden();
        var chat = await _db.Chats.AsNoTracking().SingleOrDefaultAsync(item =>
            item.Id == query.GroupId &&
            item.Status == EntityStatus.Active &&
            item.MemberIds.Contains(account.Id), ct);
        return chat is null
            ? ServiceResult<ChatGroupDto>.NotFound("Không tìm thấy nhóm trò chuyện.")
            : ServiceResult<ChatGroupDto>.Success(
                new ChatGroupDto(chat.Id, chat.MemberIds.ToArray(), chat.CreateAt));
    }
}

public sealed class AddChatMembersHandler : IRequestHandler<AddChatMembersCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public AddChatMembersHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(AddChatMembersCommand command, CancellationToken ct)
    {
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult.Forbidden();
        var chat = await _db.Chats.SingleOrDefaultAsync(item =>
            item.Id == command.GroupId &&
            item.Status == EntityStatus.Active &&
            item.MemberIds.Contains(account.Id), ct);
        if (chat is null) return ServiceResult.NotFound("Không tìm thấy nhóm trò chuyện.");
        var memberIds = (command.MemberAccountIds ?? Array.Empty<Guid>()).Distinct().ToArray();
        if (memberIds.Length == 0) return ServiceResult.Failure("Cần chỉ định ít nhất một thành viên.");
        if (!await ChatRules.AreActiveAccounts(_db, memberIds, ct))
            return ServiceResult.Failure("Một hoặc nhiều thành viên không tồn tại hoặc không hoạt động.");
        foreach (var memberId in memberIds)
        {
            var added = chat.AddMember(memberId);
            if (!added.IsSuccess) return ServiceResult.Failure(added.Message);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã thêm thành viên vào nhóm trò chuyện.");
    }
}

public sealed class SendChatMessageHandler : IRequestHandler<SendChatMessageCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public SendChatMessageHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(SendChatMessageCommand command, CancellationToken ct)
    {
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult<Guid>.Forbidden();
        var chat = await _db.Chats.SingleOrDefaultAsync(item =>
            item.Id == command.GroupId &&
            item.Status == EntityStatus.Active &&
            item.MemberIds.Contains(account.Id), ct);
        if (chat is null) return ServiceResult<Guid>.NotFound("Không tìm thấy nhóm trò chuyện.");
        var sent = chat.SendMessage(account.Id, command.Content);
        if (!sent.IsSuccess || sent.Data is null)
            return ServiceResult<Guid>.Failure(sent.Message);
        foreach (var recipientId in chat.MemberIds.Where(id => id != account.Id))
        {
            var notification = Notification.Create(
                recipientId,
                "Tin nhắn mới",
                $"Bạn có tin nhắn mới trong nhóm trò chuyện {chat.Id}.",
                NotificationType.System);
            if (!notification.IsSuccess || notification.Data is null)
                return ServiceResult<Guid>.Error(notification.Message);
            _db.Notifications.Add(notification.Data);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(sent.Data.Id, "Đã gửi tin nhắn.");
    }
}

public sealed class ListChatMessagesHandler : IRequestHandler<ListChatMessagesQuery, ServiceResult<ChatMessagesPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListChatMessagesHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ChatMessagesPageDto>> Handle(ListChatMessagesQuery query, CancellationToken ct)
    {
        if (query.PageSize is < 1 or > 100)
            return ServiceResult<ChatMessagesPageDto>.Failure("PageSize phải từ 1 đến 100.");
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult<ChatMessagesPageDto>.Forbidden();
        var chat = await _db.Chats.AsNoTracking().SingleOrDefaultAsync(item =>
            item.Id == query.GroupId &&
            item.Status == EntityStatus.Active &&
            item.MemberIds.Contains(account.Id), ct);
        if (chat is null) return ServiceResult<ChatMessagesPageDto>.NotFound("Không tìm thấy nhóm trò chuyện.");
        var messages = _db.Messages.AsNoTracking().Where(message => message.ChatId == chat.Id);
        if (query.Before.HasValue)
            messages = messages.Where(message => message.SentAtUtc < query.Before.Value.UtcDateTime);
        var items = await messages.OrderByDescending(message => message.SentAtUtc).ThenByDescending(message => message.Id)
            .Take(query.PageSize)
            .Select(message => new ChatMessageDto(
                message.Id,
                message.SenderAccountId,
                message.Status == EntityStatus.Deleted ? null : message.Content,
                message.Status == EntityStatus.Deleted ? "Withdrawn" : "Sent",
                message.SentAtUtc))
            .ToListAsync(ct);
        return ServiceResult<ChatMessagesPageDto>.Success(new ChatMessagesPageDto(items, query.PageSize));
    }
}

public sealed class WithdrawChatMessageHandler : IRequestHandler<WithdrawChatMessageCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public WithdrawChatMessageHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(WithdrawChatMessageCommand command, CancellationToken ct)
    {
        var account = await ChatRules.GetActiveAccount(_db, _session, ct);
        if (account is null) return ServiceResult.Forbidden();
        var chat = await _db.Chats.SingleOrDefaultAsync(item =>
            item.Id == command.GroupId &&
            item.Status == EntityStatus.Active &&
            item.MemberIds.Contains(account.Id), ct);
        if (chat is null) return ServiceResult.NotFound("Không tìm thấy nhóm trò chuyện.");
        var message = await _db.Messages.SingleOrDefaultAsync(item =>
            item.Id == command.MessageId &&
            item.ChatId == chat.Id &&
            item.SenderAccountId == account.Id, ct);
        if (message is null) return ServiceResult.NotFound("Không tìm thấy tin nhắn của bạn.");
        var recalled = message.Recall();
        if (!recalled.IsSuccess) return ServiceResult.Failure(recalled.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã thu hồi tin nhắn.");
    }
}

public sealed class DeleteChatGroupHandler : IRequestHandler<DeleteChatGroupCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public DeleteChatGroupHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(DeleteChatGroupCommand command, CancellationToken ct)
    {
        var admin = await ChatRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden();
        var chat = await _db.Chats.SingleOrDefaultAsync(item =>
            item.Id == command.GroupId && item.Status == EntityStatus.Active, ct);
        if (chat is null) return ServiceResult.NotFound("Không tìm thấy nhóm trò chuyện.");
        chat.UpdateStatus(EntityStatus.Deleted);
        var audit = AuditLog.Record(admin.Id, "ChatGroupDeleted", "Chat", chat.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã xoá nhóm trò chuyện.");
    }
}

public sealed class WithdrawAllChatMessagesHandler
    : IRequestHandler<WithdrawAllChatMessagesCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public WithdrawAllChatMessagesHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(WithdrawAllChatMessagesCommand command, CancellationToken ct)
    {
        var admin = await ChatRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden();
        var chat = await _db.Chats.SingleOrDefaultAsync(item => item.Id == command.GroupId, ct);
        if (chat is null) return ServiceResult.NotFound("Không tìm thấy nhóm trò chuyện.");
        var activeMessages = await _db.Messages
            .Where(message => message.ChatId == chat.Id && message.Status != EntityStatus.Deleted)
            .ToListAsync(ct);
        foreach (var message in activeMessages)
        {
            var result = message.Recall();
            if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        }
        var audit = AuditLog.Record(admin.Id, "ChatMessagesWithdrawn", "Chat", chat.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success($"Đã thu hồi {activeMessages.Count} tin nhắn.");
    }
}

public sealed class ListAuditLogsHandler : IRequestHandler<ListAuditLogsQuery, ServiceResult<AuditLogPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListAuditLogsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<AuditLogPageDto>> Handle(ListAuditLogsQuery query, CancellationToken ct)
    {
        if (await ChatRules.GetSystemAdmin(_db, _session, ct) is null)
            return ServiceResult<AuditLogPageDto>.Forbidden();
        if (!ChatRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<AuditLogPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
            return ServiceResult<AuditLogPageDto>.Failure("Khoảng thời gian lọc không hợp lệ.");
        var logs = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (query.ActorId.HasValue) logs = logs.Where(item => item.ActorAccountId == query.ActorId);
        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var action = query.Action.Trim().ToUpperInvariant();
            logs = logs.Where(item => item.Action.ToUpper() == action);
        }
        if (query.From.HasValue) logs = logs.Where(item => item.CreatedAtUtc >= query.From.Value.UtcDateTime);
        if (query.To.HasValue) logs = logs.Where(item => item.CreatedAtUtc <= query.To.Value.UtcDateTime);
        var total = await logs.CountAsync(ct);
        var items = await logs.OrderByDescending(item => item.CreatedAtUtc).ThenBy(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .Select(item => new AuditLogItemDto(
                item.Id, item.ActorAccountId, item.Action, item.EntityType, item.EntityId, item.CreatedAtUtc))
            .ToListAsync(ct);
        return ServiceResult<AuditLogPageDto>.Success(
            new AuditLogPageDto(items, query.Page, query.PageSize, total));
    }
}

internal static class ChatRules
{
    public static async Task<Account?> GetActiveAccount(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var id = session.GetUserId();
        if (!id.HasValue || id.Value == Guid.Empty) return null;
        var account = await db.Accounts.SingleOrDefaultAsync(item => item.Id == id.Value, ct);
        return account is { IsVerified: true, Status: EntityStatus.Active } ? account : null;
    }

    public static async Task<Admin?> GetSystemAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var account = await GetActiveAccount(db, session, ct);
        return account as Admin;
    }

    public static async Task<bool> AreActiveAccounts(
        IApplicationDbContext db, IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        if (ids.Count == 0 || ids.Contains(Guid.Empty)) return false;
        return await db.Accounts.CountAsync(account =>
            ids.Contains(account.Id) && account.IsVerified && account.Status == EntityStatus.Active, ct) == ids.Count;
    }

    public static bool TryPaging(int page, int pageSize, out int skip)
    {
        skip = 0;
        if (page < 1 || pageSize is < 1 or > 100) return false;
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue) return false;
        skip = (int)offset;
        return true;
    }
}
