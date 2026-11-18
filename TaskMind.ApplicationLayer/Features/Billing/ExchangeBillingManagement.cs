using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Projects;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.ObjectValues;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.ApplicationLayer.Features.Billing;

public sealed record CreateExchangeContractRequest(
    Guid PartyBAccountId,
    decimal ContractValue,
    string PaymentType,
    decimal ServiceFeePercent,
    string Currency = "VND");
public sealed record ExchangeContractActionRequest(string? Reason);
public sealed record ExchangeContractDto(
    Guid Id,
    Guid ProjectId,
    Guid PartyAAccountId,
    Guid PartyBAccountId,
    decimal ContractValue,
    string Currency,
    string PaymentType,
    decimal ServiceFeePercent,
    string Status,
    DateTimeOffset CreatedAtUtc);
public sealed record ExchangeContractPageDto(
    IReadOnlyCollection<ExchangeContractDto> Items, int Page, int PageSize, int TotalCount);
public sealed record InvoiceDto(
    Guid Id,
    string SourceType,
    Guid SourceRefId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime? PaidAtUtc,
    DateTimeOffset CreatedAtUtc);
public sealed record InvoicePageDto(
    IReadOnlyCollection<InvoiceDto> Items, int Page, int PageSize, int TotalCount);
public sealed record InvoiceStatusRequest(string? Reason);
public sealed record CreateSubscriptionInvoiceRequest(decimal Amount, string Currency = "VND");
public sealed record ListExchangeContractsQuery(Guid ProjectId, int Page, int PageSize)
    : IRequest<ServiceResult<ExchangeContractPageDto>>;
public sealed record CreateExchangeContractCommand(Guid ProjectId, CreateExchangeContractRequest Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record GetExchangeContractQuery(Guid ContractId)
    : IRequest<ServiceResult<ExchangeContractDto>>;
public sealed record ExchangeContractActionCommand(
    Guid ContractId, ExchangeContractAction Action, string? Reason) : IRequest<ServiceResult>;
public enum ExchangeContractAction { Activate, Complete, Dispute, Cancel }

public sealed record ListMyInvoicesQuery(string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<InvoicePageDto>>;
public sealed record ListInvoicesQuery(string? Status, string? SourceType, int Page, int PageSize)
    : IRequest<ServiceResult<InvoicePageDto>>;
public sealed record SetInvoiceStatusCommand(Guid InvoiceId, InvoiceStatusAction Action, string? Reason)
    : IRequest<ServiceResult>;
public sealed record CreateSubscriptionInvoiceCommand(
    InvoiceSourceType SourceType, Guid OrganizationId, decimal Amount, string Currency)
    : IRequest<ServiceResult<InvoiceDto>>;
public enum InvoiceStatusAction { Issue, Paid, Overdue }

public sealed class CreateExchangeContractHandler
    : IRequestHandler<CreateExchangeContractCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CreateExchangeContractHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateExchangeContractCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Request.PaymentType, out PaymentType? paymentType) ||
            paymentType is null)
            return ServiceResult<Guid>.Failure("Hình thức thanh toán không hợp lệ.");
        var currency = command.Request.Currency?.Trim().ToUpperInvariant();
        if (currency is not { Length: 3 } || currency.Any(character => character is < 'A' or > 'Z'))
            return ServiceResult<Guid>.Failure("Mã tiền tệ phải gồm đúng 3 chữ cái ASCII.");
        if (command.Request.ContractValue < 0)
            return ServiceResult<Guid>.Failure("Giá trị hợp đồng không được âm.");

        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<Guid>.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult<Guid>.NotFound("Không tìm thấy dự án.");
        if (!project.IsExchangeProject)
            return ServiceResult<Guid>.Failure("Chỉ dự án trao đổi thương mại mới có hợp đồng giao dịch.");
        if (project.ProjectStatus is ProjectStatus.Completed or ProjectStatus.Cancelled)
            return ServiceResult<Guid>.Failure("Không thể tạo hợp đồng cho dự án đã kết thúc.");
        if (!await ProjectRules.IsProjectOwner(_db, actor, project.Id, ct))
            return ServiceResult<Guid>.Forbidden("Chỉ Owner của dự án mới có thể đề xuất hợp đồng.");
        var partyB = await _db.Accounts.SingleOrDefaultAsync(account =>
            account.Id == command.Request.PartyBAccountId &&
            account.IsVerified && account.Status == EntityStatus.Active, ct);
        if (partyB is null || partyB.Id == actor.Id)
            return ServiceResult<Guid>.Failure("Bên B phải là một tài khoản khác đang hoạt động.");

        var contract = ExchangeContract.Create(
            project.Id,
            actor.Id,
            partyB.Id,
            Money.Of(command.Request.ContractValue, currency),
            command.Request.ServiceFeePercent,
            paymentType.Value);
        if (!contract.IsSuccess || contract.Data is null)
            return ServiceResult<Guid>.Failure(contract.Message);
        var audit = AuditLog.Record(actor.Id, "ExchangeContractProposed", "ExchangeContract", contract.Data.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult<Guid>.Error(audit.Message);
        var notification = Notification.Create(
            partyB.Id,
            "Đề xuất hợp đồng trao đổi",
            $"Owner dự án {project.Title} đã đề xuất hợp đồng. Hãy xem lại giá trị, loại thanh toán và phí dịch vụ.",
            NotificationType.System);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult<Guid>.Error(notification.Message);
        _db.ExchangeContracts.Add(contract.Data);
        _db.AuditLogs.Add(audit.Data);
        _db.Notifications.Add(notification.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(contract.Data.Id, "Đã tạo đề xuất hợp đồng trao đổi.");
    }
}

public sealed class ListExchangeContractsHandler
    : IRequestHandler<ListExchangeContractsQuery, ServiceResult<ExchangeContractPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListExchangeContractsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ExchangeContractPageDto>> Handle(
        ListExchangeContractsQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<ExchangeContractPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<ExchangeContractPageDto>.Forbidden();
        var project = await _db.Projects.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.ProjectId, ct);
        if (project is null) return ServiceResult<ExchangeContractPageDto>.NotFound("Không tìm thấy dự án.");
        if (!project.IsExchangeProject)
            return ServiceResult<ExchangeContractPageDto>.Failure("Dự án không phải dự án trao đổi thương mại.");
        var contracts = _db.ExchangeContracts.AsNoTracking()
            .Where(item => item.ProjectId == project.Id &&
                           (item.PartyAAccountId == actor.Id || item.PartyBAccountId == actor.Id));
        var total = await contracts.CountAsync(ct);
        var rows = await contracts.OrderByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .ToArrayAsync(ct);
        return ServiceResult<ExchangeContractPageDto>.Success(
            new ExchangeContractPageDto(rows.Select(ExchangeContractMapping.ToDto).ToArray(),
                query.Page, query.PageSize, total));
    }
}

public sealed class GetExchangeContractHandler
    : IRequestHandler<GetExchangeContractQuery, ServiceResult<ExchangeContractDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public GetExchangeContractHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ExchangeContractDto>> Handle(GetExchangeContractQuery query, CancellationToken ct)
    {
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<ExchangeContractDto>.Forbidden();
        var contract = await _db.ExchangeContracts.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.ContractId, ct);
        if (contract is null) return ServiceResult<ExchangeContractDto>.NotFound("Không tìm thấy hợp đồng.");
        if (contract.PartyAAccountId != actor.Id && contract.PartyBAccountId != actor.Id)
            return ServiceResult<ExchangeContractDto>.Forbidden();
        return ServiceResult<ExchangeContractDto>.Success(ExchangeContractMapping.ToDto(contract));
    }
}

public sealed class ExchangeContractActionHandler
    : IRequestHandler<ExchangeContractActionCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ExchangeContractActionHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ExchangeContractActionCommand command, CancellationToken ct)
    {
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var contract = await _db.ExchangeContracts.SingleOrDefaultAsync(
            item => item.Id == command.ContractId, ct);
        if (contract is null) return ServiceResult.NotFound("Không tìm thấy hợp đồng.");
        if (actor.Id != contract.PartyAAccountId && actor.Id != contract.PartyBAccountId)
            return ServiceResult.Forbidden("Chỉ các bên của hợp đồng mới được thực hiện thao tác này.");
        if (command.Action == ExchangeContractAction.Activate && actor.Id != contract.PartyBAccountId)
            return ServiceResult.Forbidden("Chỉ bên B mới có thể chấp nhận đề xuất hợp đồng.");
        if (command.Action == ExchangeContractAction.Complete && actor.Id != contract.PartyBAccountId)
            return ServiceResult.Forbidden("Chỉ bên B mới có thể xác nhận hoàn tất hợp đồng.");
        if (command.Action is ExchangeContractAction.Dispute or ExchangeContractAction.Cancel &&
            string.IsNullOrWhiteSpace(command.Reason))
            return ServiceResult.Failure("Cần cung cấp lý do.");

        var result = command.Action switch
        {
            ExchangeContractAction.Activate => contract.Activate(actor.Id),
            ExchangeContractAction.Complete => contract.Complete(actor.Id),
            ExchangeContractAction.Dispute => contract.Dispute(),
            ExchangeContractAction.Cancel => contract.Cancel(),
            _ => TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result.Failure(
                "Thao tác hợp đồng không hợp lệ.")
        };
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        var actionName = command.Action switch
        {
            ExchangeContractAction.Activate => "ExchangeContractActivated",
            ExchangeContractAction.Complete => "ExchangeContractCompleted",
            ExchangeContractAction.Dispute => "ExchangeContractDisputed",
            _ => "ExchangeContractCancelled"
        };
        var audit = AuditLog.Record(actor.Id, actionName, "ExchangeContract", contract.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);

        var projectTitle = await _db.Projects.AsNoTracking()
            .Where(item => item.Id == contract.ProjectId)
            .Select(item => item.Title)
            .SingleOrDefaultAsync(ct) ?? "dự án";
        var otherPartyId = actor.Id == contract.PartyAAccountId
            ? contract.PartyBAccountId
            : contract.PartyAAccountId;
        var notificationType = command.Action is ExchangeContractAction.Dispute or ExchangeContractAction.Cancel
            ? NotificationType.Warning
            : NotificationType.System;
        var notification = Notification.Create(
            otherPartyId,
            actionName,
            $"Hợp đồng của dự án {projectTitle} đã chuyển sang trạng thái {contract.ContractStatus}." +
            (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Lý do: {command.Reason.Trim()}"),
            notificationType);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult.Error(notification.Message);
        _db.Notifications.Add(notification.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success($"Đã cập nhật trạng thái hợp đồng thành {contract.ContractStatus}.");
    }
}

public sealed class ListMyInvoicesHandler
    : IRequestHandler<ListMyInvoicesQuery, ServiceResult<InvoicePageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListMyInvoicesHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<InvoicePageDto>> Handle(ListMyInvoicesQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip) ||
            !ProjectRules.TryEnum(query.Status, out InvoiceStatus? status))
            return ServiceResult<InvoicePageDto>.Failure("Phân trang hoặc trạng thái hoá đơn không hợp lệ.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<InvoicePageDto>.Forbidden();
        var invoices = _db.Invoices.AsNoTracking().AsQueryable();
        invoices = actor switch
        {
            AdminCompany adminCompany => invoices.Where(item =>
                item.SourceType == InvoiceSourceType.CompanySubscription &&
                item.SourceRefId == adminCompany.CompanyId),
            AdminSchool adminSchool => invoices.Where(item =>
                item.SourceType == InvoiceSourceType.SchoolSubscription &&
                item.SourceRefId == adminSchool.SchoolId),
            _ => invoices.Where(item =>
                item.SourceType == InvoiceSourceType.ExchangeFee &&
                _db.ExchangeContracts.Any(contract =>
                    contract.Id == item.SourceRefId &&
                    (contract.PartyAAccountId == actor.Id || contract.PartyBAccountId == actor.Id)))
        };
        if (status.HasValue) invoices = invoices.Where(item => item.InvoiceStatus == status.Value);
        var total = await invoices.CountAsync(ct);
        var rows = await invoices.OrderByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<InvoicePageDto>.Success(
            new InvoicePageDto(rows.Select(InvoiceMapping.ToDto).ToArray(),
                query.Page, query.PageSize, total));
    }
}

public sealed class ListInvoicesHandler
    : IRequestHandler<ListInvoicesQuery, ServiceResult<InvoicePageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListInvoicesHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<InvoicePageDto>> Handle(ListInvoicesQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip) ||
            !ProjectRules.TryEnum(query.Status, out InvoiceStatus? status) ||
            !ProjectRules.TryEnum(query.SourceType, out InvoiceSourceType? sourceType))
            return ServiceResult<InvoicePageDto>.Failure("Bộ lọc hoặc phân trang hoá đơn không hợp lệ.");
        if (await ProjectRules.GetSystemAdmin(_db, _session, ct) is null)
            return ServiceResult<InvoicePageDto>.Forbidden();
        var invoices = _db.Invoices.AsNoTracking().AsQueryable();
        if (status.HasValue) invoices = invoices.Where(item => item.InvoiceStatus == status.Value);
        if (sourceType.HasValue) invoices = invoices.Where(item => item.SourceType == sourceType.Value);
        var total = await invoices.CountAsync(ct);
        var rows = await invoices.OrderByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<InvoicePageDto>.Success(
            new InvoicePageDto(rows.Select(InvoiceMapping.ToDto).ToArray(),
                query.Page, query.PageSize, total));
    }
}

public sealed class SetInvoiceStatusHandler : IRequestHandler<SetInvoiceStatusCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public SetInvoiceStatusHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(SetInvoiceStatusCommand command, CancellationToken ct)
    {
        var admin = await ProjectRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden();
        var invoice = await _db.Invoices.SingleOrDefaultAsync(item => item.Id == command.InvoiceId, ct);
        if (invoice is null) return ServiceResult.NotFound("Không tìm thấy hoá đơn.");
        var transition = command.Action switch
        {
            InvoiceStatusAction.Issue => invoice.MarkAsIssued(),
            InvoiceStatusAction.Paid => invoice.MarkAsPaid(),
            InvoiceStatusAction.Overdue => invoice.MarkAsOverdue(),
            _ => TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result.Failure(
                "Thao tác hoá đơn không hợp lệ.")
        };
        if (!transition.IsSuccess) return ServiceResult.Failure(transition.Message);
        var action = command.Action switch
        {
            InvoiceStatusAction.Issue => "InvoiceIssued",
            InvoiceStatusAction.Paid => "InvoiceMarkedPaid",
            _ => "InvoiceMarkedOverdue"
        };
        var audit = AuditLog.Record(admin.Id, action, "Invoice", invoice.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success($"Hoá đơn đã chuyển sang trạng thái {invoice.InvoiceStatus}.");
    }
}

public sealed class CreateSubscriptionInvoiceHandler
    : IRequestHandler<CreateSubscriptionInvoiceCommand, ServiceResult<InvoiceDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public CreateSubscriptionInvoiceHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<InvoiceDto>> Handle(
        CreateSubscriptionInvoiceCommand command, CancellationToken ct)
    {
        var admin = await ProjectRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult<InvoiceDto>.Forbidden();
        if (command.SourceType is not (InvoiceSourceType.CompanySubscription or InvoiceSourceType.SchoolSubscription))
            return ServiceResult<InvoiceDto>.Failure("Chỉ có thể phát hành hoá đơn Subscription cho Company hoặc School.");
        if (command.Amount <= 0)
            return ServiceResult<InvoiceDto>.Failure("Số tiền hoá đơn phải lớn hơn 0.");
        var currency = command.Currency?.Trim().ToUpperInvariant();
        if (currency is not { Length: 3 } ||
            currency.Any(character => character is < 'A' or > 'Z'))
            return ServiceResult<InvoiceDto>.Failure("Mã tiền tệ phải gồm đúng 3 chữ cái ASCII.");

        var organizationIsActive = command.SourceType switch
        {
            InvoiceSourceType.CompanySubscription => await _db.Companies.AnyAsync(item =>
                item.Id == command.OrganizationId && item.IsVerified && item.Status == EntityStatus.Active, ct),
            InvoiceSourceType.SchoolSubscription => await _db.Schools.AnyAsync(item =>
                item.Id == command.OrganizationId && item.IsVerified && item.Status == EntityStatus.Active, ct),
            _ => false
        };
        if (!organizationIsActive)
            return ServiceResult<InvoiceDto>.NotFound("Không tìm thấy tổ chức đang hoạt động.");

        var created = Invoice.Create(
            command.SourceType, command.OrganizationId, Money.Of(command.Amount, currency));
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<InvoiceDto>.Failure(created.Message);
        var issued = created.Data.MarkAsIssued();
        if (!issued.IsSuccess)
            return ServiceResult<InvoiceDto>.Failure(issued.Message);
        var audit = AuditLog.Record(admin.Id, "SubscriptionInvoiceIssued", nameof(Invoice), created.Data.Id);
        if (!audit.IsSuccess || audit.Data is null)
            return ServiceResult<InvoiceDto>.Error(audit.Message);

        _db.Invoices.Add(created.Data);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<InvoiceDto>.Success(
            InvoiceMapping.ToDto(created.Data), "Đã phát hành hoá đơn Subscription.");
    }
}

public sealed class ExchangeContractCompletedEventHandler
    : INotificationHandler<ExchangeContractCompletedEvent>
{
    private readonly IApplicationDbContext _db;
    public ExchangeContractCompletedEventHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ExchangeContractCompletedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ServiceFeeAmount == 0) return;
        if (await _db.Invoices.AnyAsync(item =>
                item.SourceType == InvoiceSourceType.ExchangeFee &&
                item.SourceRefId == notification.ExchangeContractId, cancellationToken))
            return;
        var created = Invoice.Create(
            InvoiceSourceType.ExchangeFee,
            notification.ExchangeContractId,
            Money.Of(notification.ServiceFeeAmount, notification.Currency));
        if (!created.IsSuccess || created.Data is null)
            throw new InvalidOperationException(
                $"Could not create exchange-fee invoice for contract {notification.ExchangeContractId}: {created.Message}");
        _db.Invoices.Add(created.Data);
    }
}

public sealed class InvoiceLifecycleNotificationHandler :
    INotificationHandler<InvoiceIssuedEvent>,
    INotificationHandler<InvoicePaidEvent>,
    INotificationHandler<InvoiceOverdueEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<InvoiceLifecycleNotificationHandler> _logger;
    public InvoiceLifecycleNotificationHandler(
        IApplicationDbContext db, ILogger<InvoiceLifecycleNotificationHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task Handle(InvoiceIssuedEvent notification, CancellationToken cancellationToken) =>
        NotifyAsync(notification.InvoiceId, "Hoá đơn đã phát hành", cancellationToken);

    public Task Handle(InvoicePaidEvent notification, CancellationToken cancellationToken) =>
        NotifyAsync(notification.InvoiceId, "Hoá đơn đã thanh toán", cancellationToken);

    public Task Handle(InvoiceOverdueEvent notification, CancellationToken cancellationToken) =>
        NotifyAsync(notification.InvoiceId, "Hoá đơn đã quá hạn", cancellationToken);

    private async Task NotifyAsync(Guid invoiceId, string title, CancellationToken ct)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == invoiceId, ct);
        if (invoice is null)
        {
            _logger.LogError("Invoice {InvoiceId} was not found for its lifecycle notification", invoiceId);
            return;
        }

        Guid[] recipients;
        switch (invoice.SourceType)
        {
            case InvoiceSourceType.ExchangeFee:
                var parties = await _db.ExchangeContracts.AsNoTracking()
                    .Where(item => item.Id == invoice.SourceRefId)
                    .Select(item => new { item.PartyAAccountId, item.PartyBAccountId })
                    .SingleOrDefaultAsync(ct);
                recipients = parties is null
                    ? Array.Empty<Guid>()
                    : new[] { parties.PartyAAccountId, parties.PartyBAccountId }.Distinct().ToArray();
                break;
            case InvoiceSourceType.CompanySubscription:
                recipients = await _db.AdminCompanies.AsNoTracking()
                    .Where(item => item.CompanyId == invoice.SourceRefId)
                    .Select(item => item.Id).ToArrayAsync(ct);
                break;
            case InvoiceSourceType.SchoolSubscription:
                recipients = await _db.AdminSchools.AsNoTracking()
                    .Where(item => item.SchoolId == invoice.SourceRefId)
                    .Select(item => item.Id).ToArrayAsync(ct);
                break;
            default:
                recipients = Array.Empty<Guid>();
                break;
        }
        if (recipients.Length == 0)
        {
            _logger.LogWarning(
                "No recipients found for invoice {InvoiceId} with source {SourceType}/{SourceRefId}",
                invoice.Id, invoice.SourceType, invoice.SourceRefId);
            return;
        }

        var message = $"Hoá đơn {invoice.Id} ({invoice.Amount.Amount:0.00} {invoice.Amount.Currency}) " +
                      $"đã chuyển sang trạng thái {invoice.InvoiceStatus}.";
        foreach (var recipientId in recipients)
        {
            var result = Notification.Create(
                recipientId,
                title,
                message,
                invoice.InvoiceStatus == InvoiceStatus.Overdue
                    ? NotificationType.Warning
                    : NotificationType.System);
            if (!result.IsSuccess || result.Data is null)
                throw new InvalidOperationException(
                    $"Could not create invoice notification for {recipientId}: {result.Message}");
            _db.Notifications.Add(result.Data);
        }
    }
}

internal static class ExchangeContractMapping
{
    public static ExchangeContractDto ToDto(ExchangeContract item) => new(
        item.Id,
        item.ProjectId,
        item.PartyAAccountId,
        item.PartyBAccountId,
        item.ContractValue.Amount,
        item.ContractValue.Currency,
        item.PaymentType.ToString(),
        item.ServiceFeePercent,
        item.ContractStatus.ToString(),
        item.CreateAt);
}

internal static class InvoiceMapping
{
    public static InvoiceDto ToDto(Invoice item) => new(
        item.Id,
        item.SourceType.ToString(),
        item.SourceRefId,
        item.Amount.Amount,
        item.Amount.Currency,
        item.InvoiceStatus.ToString(),
        item.PaidAtUtc,
        item.CreateAt);
}
