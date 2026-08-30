using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Dashbroads
{
    /// <summary>
    /// Admin xem tổng quan hệ thống: doanh thu (mục 4.14), số lượng công ty/cơ sở đào tạo
    /// đã duyệt/chờ duyệt (mục 4.4, 4.8), số lượng người dùng, trạng thái dự án, hoá đơn gần đây.
    /// </summary>
    public class GetDashboardQuery : ServiceResult<DashboardOverviewDto>
    {
        /// <summary>Khoảng thời gian tính doanh thu theo tháng. Mặc định: 6 tháng gần nhất nếu không truyền.</summary>
        public DateTime? FromDateUtc { get; }
        public DateTime? ToDateUtc { get; }

        public GetDashboardQuery(DateTime? fromDateUtc = null, DateTime? toDateUtc = null)
        {
            FromDateUtc = fromDateUtc;
            ToDateUtc = toDateUtc;
        }
    }

    public class GetDashboardHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetDashboardHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<DashboardOverviewDto>> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
        {
            var toDate = query.ToDateUtc ?? DateTime.UtcNow;
            var fromDate = query.FromDateUtc ?? toDate.AddMonths(-5).Date;

            var invoicesQuery = _dbContext.Invoices.AsNoTracking();
            var paidInvoices = invoicesQuery.Where(i => i.InvoiceStatus == InvoiceStatus.Paid);

            // --- Doanh thu ---
            var totalRevenue = await paidInvoices.SumAsync(i => i.Amount.Amount, cancellationToken);

            var revenueBySource = await paidInvoices
                .GroupBy(i => i.SourceType)
                .Select(g => new RevenueBySourceDto
                {
                    SourceType = g.Key,
                    TotalAmount = g.Sum(x => x.Amount.Amount),
                    InvoiceCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            // Đảm bảo đủ 3 nguồn thu (ExchangeFee/CompanySubscription/SchoolSubscription) dù = 0
            foreach (InvoiceSourceType type in Enum.GetValues(typeof(InvoiceSourceType)))
            {
                if (!revenueBySource.Any(r => r.SourceType == type))
                    revenueBySource.Add(new RevenueBySourceDto { SourceType = type, TotalAmount = 0, InvoiceCount = 0 });
            }

            var monthlyRevenue = await paidInvoices
                .Where(i => i.PaidAtUtc != null && i.PaidAtUtc >= fromDate && i.PaidAtUtc <= toDate)
                .GroupBy(i => new { i.PaidAtUtc!.Value.Year, i.PaidAtUtc!.Value.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(x => x.Amount.Amount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);

            // --- Công ty / Cơ sở đào tạo (mục 4.4, 4.8) ---
            var totalCompanies = await _dbContext.Companies.CountAsync(cancellationToken);
            var verifiedCompanies = await _dbContext.Companies.CountAsync(c => c.IsVerified, cancellationToken);

            var totalSchools = await _dbContext.Schools.CountAsync(cancellationToken);
            var verifiedSchools = await _dbContext.Schools.CountAsync(s => s.IsVerified, cancellationToken);

            // --- Người dùng ---
            var totalUsers = await _dbContext.Users.CountAsync(cancellationToken);
            var totalStaffs = await _dbContext.Staffs.CountAsync(cancellationToken);
            var totalStudents = await _dbContext.Students.CountAsync(cancellationToken);
            var totalTeachers = await _dbContext.Teachers.CountAsync(cancellationToken);

            // --- Dự án & Trao đổi (mục 4.7/4.12/4.15) ---
            var projectsByStatus = await _dbContext.Projects
                .GroupBy(p => p.ProjectStatus)
                .Select(g => new ProjectStatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var activeExchangeContracts = await _dbContext.ExchangeContracts
                .CountAsync(e => e.ContractStatus == ExchangeStatus.Active, cancellationToken);

            // --- Hoá đơn gần đây ---
            var recentInvoices = await invoicesQuery
                .OrderByDescending(i => i.CreatedAtUtc)
                .Take(10)
                .Select(i => new RecentInvoiceDto
                {
                    Id = i.Id,
                    SourceType = i.SourceType,
                    SourceRefId = i.SourceRefId,
                    Amount = i.Amount.Amount,
                    Currency = i.Amount.Currency,
                    Status = i.InvoiceStatus,
                    CreatedAtUtc = i.CreatedAtUtc,
                    PaidAtUtc = i.PaidAtUtc
                })
                .ToListAsync(cancellationToken);

            var dto = new DashboardOverviewDto
            {
                TotalRevenue = totalRevenue,
                RevenueBySource = revenueBySource,
                MonthlyRevenue = monthlyRevenue,
                TotalCompanies = totalCompanies,
                VerifiedCompanies = verifiedCompanies,
                PendingCompanies = totalCompanies - verifiedCompanies,
                TotalSchools = totalSchools,
                VerifiedSchools = verifiedSchools,
                PendingSchools = totalSchools - verifiedSchools,
                TotalUsers = totalUsers,
                TotalStaffs = totalStaffs,
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                ProjectsByStatus = projectsByStatus,
                ActiveExchangeContracts = activeExchangeContracts,
                RecentInvoices = recentInvoices
            };

            return ServiceResult<DashboardOverviewDto>.Success(dto, "Lấy dữ liệu dashboard thành công");
        }
    }
}