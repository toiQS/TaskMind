using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Text.Json;
using TaskMind.ApplicationLayer.Features.Auths;

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
    public IActionResult ListChatGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListChatGroups));

    [HttpPost("chat-groups")]
    [EndpointSummary("Create Chat Group")]
    public IActionResult CreateChatGroup([FromBody] JsonElement request) => NotImplemented(nameof(CreateChatGroup));

    [HttpGet("chat-groups/{groupId:guid}")]
    [EndpointSummary("Get Chat Group")]
    public IActionResult GetChatGroup(Guid groupId) => NotImplemented(nameof(GetChatGroup));

    [HttpPost("chat-groups/{groupId:guid}/members")]
    [EndpointSummary("Add Chat Group Members")]
    public IActionResult AddChatGroupMembers(Guid groupId, [FromBody] JsonElement request) => NotImplemented(nameof(AddChatGroupMembers));

    [HttpPost("chat-groups/{groupId:guid}/messages")]
    [EndpointSummary("Send Message")]
    public IActionResult SendMessage(Guid groupId, [FromBody] JsonElement request) => NotImplemented(nameof(SendMessage));

    [HttpGet("chat-groups/{groupId:guid}/messages")]
    [EndpointSummary("List Messages")]
    public IActionResult ListMessages(Guid groupId, [FromQuery] DateTimeOffset? before, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListMessages));

    [HttpPost("chat-groups/{groupId:guid}/messages/{messageId:guid}/withdraw")]
    [EndpointSummary("Withdraw Message")]
    public IActionResult WithdrawMessage(Guid groupId, Guid messageId, [FromBody] JsonElement request) => NotImplemented(nameof(WithdrawMessage));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/chat-groups/{groupId:guid}/deletion")]
    [EndpointSummary("Delete Chat Group")]
    public IActionResult DeleteChatGroup(Guid groupId, [FromBody] JsonElement request) => NotImplemented(nameof(DeleteChatGroup));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/chat-groups/{groupId:guid}/message-withdrawals")]
    [EndpointSummary("Withdraw All Group Messages")]
    public IActionResult WithdrawAllGroupMessages(Guid groupId, [FromBody] JsonElement request) => NotImplemented(nameof(WithdrawAllGroupMessages));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/audit-logs")]
    [EndpointSummary("List Audit Logs")]
    public IActionResult ListAuditLogs([FromQuery] Guid? actorId, [FromQuery] string? action, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListAuditLogs));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/chat-groups/{groupId:guid}/access-requests")]
    [EndpointSummary("Request Administrative Chat Access")]
    public IActionResult RequestAdministrativeChatAccess(Guid groupId, [FromBody] JsonElement request) => NotImplemented(nameof(RequestAdministrativeChatAccess));
}
