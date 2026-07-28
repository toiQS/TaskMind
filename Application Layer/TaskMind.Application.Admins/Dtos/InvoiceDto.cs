namespace TaskMind.Applications.Admins.Dtos
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid PartnerId { get; set; }

        /// <summary>"Company" | "School"</summary>
        public string PartnerType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// "TransactionFee" khi hoá đơn gắn với một ExchangeContract (mục 4.14, Nguồn thu 1),
        /// ngược lại "MembershipFee" (mục 4.13, Nguồn thu 2).
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>Pending | Issued | Paid | Overdue</summary>
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDateUtc { get; set; }
        public DateTime? PaidDateUtc { get; set; }
    }

    public class ProfitSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TransactionFeeRevenue { get; set; }
        public decimal MembershipFeeRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public List<InvoiceDto> RecentInvoices { get; set; } = new();
    }
}