using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Text.Json;
using TaskMind.ApplicationLayer.Features.Auths;
using TaskMind.ApplicationLayer.Features.Communication;

namespace TaskMind.APIs.Controllers;

public sealed class CommunicationController : ScaffoldControllerBase
{
    private readonly ISender _sender;

    public CommunicationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me/notifications")]
    [EndpointSummary("List My Notifications")]
    public async Task<IActionResult> ListMyNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListMyNotificationsQuery(isRead, page, pageSize), cancellationToken));

    [HttpPost("me/notifications/{notificationId:guid}/read")]
    [EndpointSummary("Mark Notification Read")]
    public async Task<IActionResult> MarkNotificationRead(Guid notificationId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new MarkMyNotificationReadCommand(notificationId), cancellationToken));

    [HttpPost("me/notifications/read-all")]
    [EndpointSummary("Mark All Notifications Read")]
    public async Task<IActionResult> MarkAllNotificationsRead(CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new MarkAllMyNotificationsReadCommand(), cancellationToken));

    [HttpGet("chat-groups")]
    [EndpointSummary("List Chat Groups")]
    public async Task<IActionResult> ListChatGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListChatGroupsQuery(page, pageSize), cancellationToken));

    [HttpPost("chat-groups")]
    [EndpointSummary("Create Chat Group")]
    public async Task<IActionResult> CreateChatGroup([FromBody] CreateChatGroupRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateChatGroupCommand(request.MemberAccountIds), cancellationToken));

    [HttpGet("chat-groups/{groupId:guid}")]
    [EndpointSummary("Get Chat Group")]
    public async Task<IActionResult> GetChatGroup(Guid groupId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetChatGroupQuery(groupId), cancellationToken));

    [HttpPost("chat-groups/{groupId:guid}/members")]
    [EndpointSummary("Add Chat Group Members")]
    public async Task<IActionResult> AddChatGroupMembers(Guid groupId, [FromBody] AddChatMembersRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new AddChatMembersCommand(groupId, request.MemberAccountIds), cancellationToken));

    [HttpPost("chat-groups/{groupId:guid}/messages")]
    [EndpointSummary("Send Message")]
    public async Task<IActionResult> SendMessage(Guid groupId, [FromBody] SendChatMessageRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new SendChatMessageCommand(groupId, request.Content), cancellationToken));

    [HttpGet("chat-groups/{groupId:guid}/messages")]
    [EndpointSummary("List Messages")]
    public async Task<IActionResult> ListMessages(Guid groupId, [FromQuery] DateTimeOffset? before, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListChatMessagesQuery(groupId, before, pageSize), cancellationToken));

    [HttpPost("chat-groups/{groupId:guid}/messages/{messageId:guid}/withdraw")]
    [EndpointSummary("Withdraw Message")]
    public async Task<IActionResult> WithdrawMessage(Guid groupId, Guid messageId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new WithdrawChatMessageCommand(groupId, messageId), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/chat-groups/{groupId:guid}/deletion")]
    [EndpointSummary("Delete Chat Group")]
    public async Task<IActionResult> DeleteChatGroup(Guid groupId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new DeleteChatGroupCommand(groupId), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/chat-groups/{groupId:guid}/message-withdrawals")]
    [EndpointSummary("Withdraw All Group Messages")]
    public async Task<IActionResult> WithdrawAllGroupMessages(Guid groupId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new WithdrawAllChatMessagesCommand(groupId), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/audit-logs")]
    [EndpointSummary("List Audit Logs")]
    public async Task<IActionResult> ListAuditLogs([FromQuery] Guid? actorId, [FromQuery] string? action, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListAuditLogsQuery(actorId, action, from, to, page, pageSize), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/chat-groups/{groupId:guid}/access-requests")]
    [EndpointSummary("Request Administrative Chat Access")]
    public IActionResult RequestAdministrativeChatAccess(Guid groupId, [FromBody] JsonElement request) => NotImplemented(nameof(RequestAdministrativeChatAccess));
}
