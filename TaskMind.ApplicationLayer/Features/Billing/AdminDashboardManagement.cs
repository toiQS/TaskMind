using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Billing;

public sealed record ProjectStatusCountDto(string Status, int Count);
public sealed record RevenueBucketDto(
    string? Period, string? SourceType, string Currency, decimal Amount, int InvoiceCount);
public sealed record AdminDashboardDto(
    int ApprovedCompanies,
    int PendingCompanies,
    int ApprovedSchools,
    int PendingSchools,
    int Users,
    int Staff,
    int Students,
    int Teachers,
    IReadOnlyCollection<ProjectStatusCountDto> ProjectsByStatus,
    int ActiveExchangeContracts,
    IReadOnlyCollection<InvoiceDto> RecentInvoices);
public sealed record RevenueDashboardDto(
    DateOnly From,
    DateOnly To,
    string GroupBy,
    IReadOnlyCollection<RevenueBucketDto> Items);
public sealed record AdminAccountDashboardDto(int Users, int Staff, int Students, int Teachers);
public sealed record AdminProjectDashboardDto(IReadOnlyCollection<ProjectStatusCountDto> Items);

public sealed record GetAdminDashboardQuery : IRequest<ServiceResult<AdminDashboardDto>>;
public sealed record GetAdminRevenueDashboardQuery(DateOnly? From, DateOnly? To, string? GroupBy)
    : IRequest<ServiceResult<RevenueDashboardDto>>;
public sealed record GetAdminProjectDashboardQuery : IRequest<ServiceResult<AdminProjectDashboardDto>>;
public sealed record GetAdminAccountDashboardQuery : IRequest<ServiceResult<AdminAccountDashboardDto>>;
public sealed record GetRecentAdminInvoicesQuery(int Count) : IRequest<ServiceResult<InvoicePageDto>>;

public sealed class GetAdminDashboardHandler
    : IRequestHandler<GetAdminDashboardQuery, ServiceResult<AdminDashboardDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public GetAdminDashboardHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<AdminDashboardDto>> Handle(
        GetAdminDashboardQuery query, CancellationToken ct)
    {
        if (await AdminDashboardRules.GetAdmin(_db, _session, ct) is null)
            return ServiceResult<AdminDashboardDto>.Forbidden();

        var projects = await AdminDashboardRules.GetProjectCounts(_db, ct);
        var result = new AdminDashboardDto(
            await _db.Companies.CountAsync(item => item.IsVerified, ct),
            await _db.Companies.CountAsync(item => !item.IsVerified && item.Status == EntityStatus.Paused, ct),
            await _db.Schools.CountAsync(item => item.IsVerified, ct),
            await _db.Schools.CountAsync(item => !item.IsVerified && item.Status == EntityStatus.Paused, ct),
            await _db.Users.CountAsync(ct),
            await _db.Staffs.CountAsync(ct),
            await _db.Students.CountAsync(ct),
            await _db.Teachers.CountAsync(ct),
            projects,
            await _db.ExchangeContracts.CountAsync(item => item.ContractStatus == ExchangeStatus.Active, ct),
            await AdminDashboardRules.GetRecentInvoices(_db, 10, ct));
        return ServiceResult<AdminDashboardDto>.Success(result);
    }
}

public sealed class GetAdminRevenueDashboardHandler
    : IRequestHandler<GetAdminRevenueDashboardQuery, ServiceResult<RevenueDashboardDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public GetAdminRevenueDashboardHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<RevenueDashboardDto>> Handle(
        GetAdminRevenueDashboardQuery query, CancellationToken ct)
    {
        if (await AdminDashboardRules.GetAdmin(_db, _session, ct) is null)
            return ServiceResult<RevenueDashboardDto>.Forbidden();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = query.From ?? new DateOnly(today.Year, today.Month, 1).AddMonths(-11);
        var to = query.To ?? today;
        if (from > to || to.DayNumber - from.DayNumber > 366 * 5)
            return ServiceResult<RevenueDashboardDto>.Failure(
                "Khoảng thời gian phải hợp lệ và không vượt quá 5 năm.");

        var groupBy = string.IsNullOrWhiteSpace(query.GroupBy)
            ? "source-month"
            : query.GroupBy.Trim().ToLowerInvariant();
        if (groupBy is not ("month" or "source" or "source-month"))
            return ServiceResult<RevenueDashboardDto>.Failure(
                "groupBy phải là month, source hoặc source-month.");

        var startUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var paymentsQuery = _db.Invoices.AsNoTracking()
            .Where(item => item.InvoiceStatus == InvoiceStatus.Paid &&
                           item.PaidAtUtc.HasValue &&
                           item.PaidAtUtc.Value >= startUtc);
        if (to == DateOnly.MaxValue)
        {
            var endInclusiveUtc = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            paymentsQuery = paymentsQuery.Where(item => item.PaidAtUtc!.Value <= endInclusiveUtc);
        }
        else
        {
            var endExclusiveUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            paymentsQuery = paymentsQuery.Where(item => item.PaidAtUtc!.Value < endExclusiveUtc);
        }
        var payments = await paymentsQuery
            .Select(item => new
            {
                item.PaidAtUtc,
                item.SourceType,
                item.Amount.Amount,
                item.Amount.Currency
            })
            .ToListAsync(ct);

        var buckets = payments
            .Select(item => new
            {
                Period = item.PaidAtUtc!.Value.ToString("yyyy-MM"),
                SourceType = item.SourceType.ToString(),
                item.Currency,
                item.Amount
            })
            .GroupBy(item => new
            {
                Period = groupBy == "source" ? null : item.Period,
                SourceType = groupBy == "month" ? null : item.SourceType,
                item.Currency
            })
            .OrderBy(group => group.Key.Period)
            .ThenBy(group => group.Key.SourceType)
            .ThenBy(group => group.Key.Currency)
            .Select(group => new RevenueBucketDto(
                group.Key.Period,
                group.Key.SourceType,
                group.Key.Currency,
                group.Sum(item => item.Amount),
                group.Count()))
            .ToArray();

        return ServiceResult<RevenueDashboardDto>.Success(
            new RevenueDashboardDto(from, to, groupBy, buckets));
    }
}

public sealed class GetAdminProjectDashboardHandler
    : IRequestHandler<GetAdminProjectDashboardQuery, ServiceResult<AdminProjectDashboardDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public GetAdminProjectDashboardHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<AdminProjectDashboardDto>> Handle(
        GetAdminProjectDashboardQuery query, CancellationToken ct)
    {
        if (await AdminDashboardRules.GetAdmin(_db, _session, ct) is null)
            return ServiceResult<AdminProjectDashboardDto>.Forbidden();
        return ServiceResult<AdminProjectDashboardDto>.Success(
            new AdminProjectDashboardDto(await AdminDashboardRules.GetProjectCounts(_db, ct)));
    }
}

public sealed class GetAdminAccountDashboardHandler
    : IRequestHandler<GetAdminAccountDashboardQuery, ServiceResult<AdminAccountDashboardDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public GetAdminAccountDashboardHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<AdminAccountDashboardDto>> Handle(
        GetAdminAccountDashboardQuery query, CancellationToken ct)
    {
        if (await AdminDashboardRules.GetAdmin(_db, _session, ct) is null)
            return ServiceResult<AdminAccountDashboardDto>.Forbidden();
        return ServiceResult<AdminAccountDashboardDto>.Success(new AdminAccountDashboardDto(
            await _db.Users.CountAsync(ct),
            await _db.Staffs.CountAsync(ct),
            await _db.Students.CountAsync(ct),
            await _db.Teachers.CountAsync(ct)));
    }
}

public sealed class GetRecentAdminInvoicesHandler
    : IRequestHandler<GetRecentAdminInvoicesQuery, ServiceResult<InvoicePageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public GetRecentAdminInvoicesHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<InvoicePageDto>> Handle(
        GetRecentAdminInvoicesQuery query, CancellationToken ct)
    {
        if (await AdminDashboardRules.GetAdmin(_db, _session, ct) is null)
            return ServiceResult<InvoicePageDto>.Forbidden();
        if (query.Count is < 1 or > 50)
            return ServiceResult<InvoicePageDto>.Failure("count phải từ 1 đến 50.");

        var items = await AdminDashboardRules.GetRecentInvoices(_db, query.Count, ct);
        var total = await _db.Invoices.CountAsync(ct);
        return ServiceResult<InvoicePageDto>.Success(
            new InvoicePageDto(items, 1, query.Count, total));
    }
}

internal static class AdminDashboardRules
{
    public static async Task<Admin?> GetAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var accountId = session.GetUserId();
        if (!accountId.HasValue || accountId.Value == Guid.Empty)
            return null;
        var admin = await db.Admins.SingleOrDefaultAsync(item => item.Id == accountId.Value, ct);
        return admin is { IsVerified: true, Status: EntityStatus.Active } ? admin : null;
    }

    public static async Task<IReadOnlyCollection<ProjectStatusCountDto>> GetProjectCounts(
        IApplicationDbContext db, CancellationToken ct)
    {
        var counts = await db.Projects.AsNoTracking()
            .GroupBy(item => item.ProjectStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, ct);
        return Enum.GetValues<ProjectStatus>()
            .Select(status => new ProjectStatusCountDto(status.ToString(),
                counts.TryGetValue(status, out var count) ? count : 0))
            .ToArray();
    }

    public static async Task<IReadOnlyCollection<InvoiceDto>> GetRecentInvoices(
        IApplicationDbContext db, int count, CancellationToken ct)
    {
        var items = await db.Invoices.AsNoTracking()
            .OrderByDescending(item => item.CreateAt)
            .ThenByDescending(item => item.Id)
            .Take(count)
            .ToListAsync(ct);
        return items.Select(InvoiceMapping.ToDto).ToArray();
    }
}
