using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TaskMind.ApplicationLayer.Features.Billing;

namespace TaskMind.APIs.Controllers;

public sealed class BillingAndAdminController : ScaffoldControllerBase
{
    private readonly ISender _sender;
    public BillingAndAdminController(ISender sender) => _sender = sender;

    [HttpPost("projects/{projectId:guid}/exchange-contracts")]
    [EndpointSummary("Create Exchange Contract")]
    public async Task<IActionResult> CreateExchangeContract(
        Guid projectId, [FromBody] CreateExchangeContractRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateExchangeContractCommand(projectId, request), cancellationToken));

    [HttpGet("projects/{projectId:guid}/exchange-contracts")]
    [EndpointSummary("List Project Exchange Contracts")]
    public async Task<IActionResult> ListProjectExchangeContracts(
        Guid projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListExchangeContractsQuery(projectId, page, pageSize), cancellationToken));

    [HttpGet("exchange-contracts/{contractId:guid}")]
    [EndpointSummary("Get Exchange Contract")]
    public async Task<IActionResult> GetExchangeContract(Guid contractId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetExchangeContractQuery(contractId), cancellationToken));

    [HttpPost("exchange-contracts/{contractId:guid}/activation")]
    [EndpointSummary("Activate Exchange Contract")]
    public async Task<IActionResult> ActivateExchangeContract(
        Guid contractId, [FromBody] ExchangeContractActionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new ExchangeContractActionCommand(contractId, ExchangeContractAction.Activate, request.Reason),
            cancellationToken));

    [HttpPost("exchange-contracts/{contractId:guid}/completion")]
    [EndpointSummary("Complete Exchange Contract")]
    public async Task<IActionResult> CompleteExchangeContract(
        Guid contractId, [FromBody] ExchangeContractActionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new ExchangeContractActionCommand(contractId, ExchangeContractAction.Complete, request.Reason),
            cancellationToken));

    [HttpPost("exchange-contracts/{contractId:guid}/disputes")]
    [EndpointSummary("Open Exchange Dispute")]
    public async Task<IActionResult> OpenExchangeDispute(
        Guid contractId, [FromBody] ExchangeContractActionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new ExchangeContractActionCommand(contractId, ExchangeContractAction.Dispute, request.Reason),
            cancellationToken));

    [HttpPost("exchange-contracts/{contractId:guid}/cancellation")]
    [EndpointSummary("Cancel Exchange Contract")]
    public async Task<IActionResult> CancelExchangeContract(
        Guid contractId, [FromBody] ExchangeContractActionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new ExchangeContractActionCommand(contractId, ExchangeContractAction.Cancel, request.Reason),
            cancellationToken));

    [HttpGet("me/invoices")]
    [EndpointSummary("List My Invoices")]
    public async Task<IActionResult> ListMyInvoices(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListMyInvoicesQuery(status, page, pageSize), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/invoices")]
    [EndpointSummary("List Invoices")]
    public async Task<IActionResult> ListInvoices(
        [FromQuery] string? status, [FromQuery] string? sourceType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListInvoicesQuery(status, sourceType, page, pageSize), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/invoices/{invoiceId:guid}/issue")]
    [EndpointSummary("Issue Invoice")]
    public async Task<IActionResult> IssueInvoice(
        Guid invoiceId, [FromBody] InvoiceStatusRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new SetInvoiceStatusCommand(invoiceId, InvoiceStatusAction.Issue, request.Reason),
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/invoices/{invoiceId:guid}/mark-paid")]
    [EndpointSummary("Mark Invoice Paid")]
    public async Task<IActionResult> MarkInvoicePaid(
        Guid invoiceId, [FromBody] InvoiceStatusRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new SetInvoiceStatusCommand(invoiceId, InvoiceStatusAction.Paid, request.Reason),
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/invoices/{invoiceId:guid}/mark-overdue")]
    [EndpointSummary("Mark Invoice Overdue")]
    public async Task<IActionResult> MarkInvoiceOverdue(
        Guid invoiceId, [FromBody] InvoiceStatusRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new SetInvoiceStatusCommand(invoiceId, InvoiceStatusAction.Overdue, request.Reason),
            cancellationToken));

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
