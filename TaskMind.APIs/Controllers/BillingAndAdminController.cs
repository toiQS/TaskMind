using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace TaskMind.APIs.Controllers;

public sealed class BillingAndAdminController : ScaffoldControllerBase
{
    [HttpPost("projects/{projectId:guid}/exchange-contracts")]
    [EndpointSummary("Create Exchange Contract")]
    public IActionResult CreateExchangeContract(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateExchangeContract));

    [HttpGet("exchange-contracts/{contractId:guid}")]
    [EndpointSummary("Get Exchange Contract")]
    public IActionResult GetExchangeContract(Guid contractId) => NotImplemented(nameof(GetExchangeContract));

    [HttpPost("exchange-contracts/{contractId:guid}/activation")]
    [EndpointSummary("Activate Exchange Contract")]
    public IActionResult ActivateExchangeContract(Guid contractId, [FromBody] JsonElement request) => NotImplemented(nameof(ActivateExchangeContract));

    [HttpPost("exchange-contracts/{contractId:guid}/completion")]
    [EndpointSummary("Complete Exchange Contract")]
    public IActionResult CompleteExchangeContract(Guid contractId, [FromBody] JsonElement request) => NotImplemented(nameof(CompleteExchangeContract));

    [HttpPost("exchange-contracts/{contractId:guid}/disputes")]
    [EndpointSummary("Open Exchange Dispute")]
    public IActionResult OpenExchangeDispute(Guid contractId, [FromBody] JsonElement request) => NotImplemented(nameof(OpenExchangeDispute));

    [HttpPost("exchange-contracts/{contractId:guid}/cancellation")]
    [EndpointSummary("Cancel Exchange Contract")]
    public IActionResult CancelExchangeContract(Guid contractId, [FromBody] JsonElement request) => NotImplemented(nameof(CancelExchangeContract));

    [HttpGet("me/invoices")]
    [EndpointSummary("List My Invoices")]
    public IActionResult ListMyInvoices([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListMyInvoices));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/invoices")]
    [EndpointSummary("List Invoices")]
    public IActionResult ListInvoices([FromQuery] string? status, [FromQuery] string? sourceType, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListInvoices));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/invoices/{invoiceId:guid}/issue")]
    [EndpointSummary("Issue Invoice")]
    public IActionResult IssueInvoice(Guid invoiceId, [FromBody] JsonElement request) => NotImplemented(nameof(IssueInvoice));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/invoices/{invoiceId:guid}/mark-paid")]
    [EndpointSummary("Mark Invoice Paid")]
    public IActionResult MarkInvoicePaid(Guid invoiceId, [FromBody] JsonElement request) => NotImplemented(nameof(MarkInvoicePaid));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/invoices/{invoiceId:guid}/mark-overdue")]
    [EndpointSummary("Mark Invoice Overdue")]
    public IActionResult MarkInvoiceOverdue(Guid invoiceId, [FromBody] JsonElement request) => NotImplemented(nameof(MarkInvoiceOverdue));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/dashboard")]
    [EndpointSummary("Get Dashboard")]
    public IActionResult GetDashboard() => NotImplemented(nameof(GetDashboard));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/dashboard/revenue")]
    [EndpointSummary("Get Revenue Dashboard")]
    public IActionResult GetRevenueDashboard([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? groupBy) => NotImplemented(nameof(GetRevenueDashboard));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/dashboard/projects")]
    [EndpointSummary("Get Project Dashboard")]
    public IActionResult GetProjectDashboard() => NotImplemented(nameof(GetProjectDashboard));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/dashboard/accounts")]
    [EndpointSummary("Get Account Dashboard")]
    public IActionResult GetAccountDashboard() => NotImplemented(nameof(GetAccountDashboard));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/dashboard/recent-invoices")]
    [EndpointSummary("Get Recent Invoices")]
    public IActionResult GetRecentInvoices([FromQuery] int count = 10) => NotImplemented(nameof(GetRecentInvoices));
}
