using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class DashboardOverviewDto
    {
        public decimal TotalRevenue { get; set; }
        public List<RevenueBySourceDto> RevenueBySource { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();

        public int TotalCompanies { get; set; }
        public int VerifiedCompanies { get; set; }
        public int PendingCompanies { get; set; }

        public int TotalSchools { get; set; }
        public int VerifiedSchools { get; set; }
        public int PendingSchools { get; set; }

        public int TotalUsers { get; set; }
        public int TotalStaffs { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }

        public List<ProjectStatusCountDto> ProjectsByStatus { get; set; } = new();
        public int ActiveExchangeContracts { get; set; }

        public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
    }

    public class RevenueBySourceDto
    {
        public InvoiceSourceType SourceType { get; set; }
        public decimal TotalAmount { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ProjectStatusCountDto
    {
        public ProjectStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class RecentInvoiceDto
    {
        public Guid Id { get; set; }
        public InvoiceSourceType SourceType { get; set; }
        public Guid SourceRefId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public InvoiceStatus Status { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    }
}