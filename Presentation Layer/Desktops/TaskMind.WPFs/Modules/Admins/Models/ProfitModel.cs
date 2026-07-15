using System;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum ProfitSource
    {
        TransactionFee,  // Phí khấu trừ giao dịch trao đổi (mục 4.14)
        MembershipFee    // Phí tham gia hệ thống của Company/School (mục 4.4, 4.8)
    }

    public enum InvoiceStatus
    {
        Pending,  // Chờ xuất hoá đơn
        Issued,   // Đã xuất hoá đơn
        Paid,     // Đã thanh toán
        Overdue   // Quá hạn
    }

    public enum PartnerType
    {
        Company,
        School
    }

    public class ProfitSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TransactionFeeRevenue { get; set; }
        public decimal MembershipFeeRevenue { get; set; }

        /// <summary>% tăng trưởng so với kỳ trước, có thể âm</summary>
        public double GrowthPercent { get; set; }
    }

    public class ProfitTransactionModel
    {
        public string Id { get; set; }
        public string PartnerName { get; set; }
        public PartnerType PartnerType { get; set; }
        public ProfitSource Source { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public InvoiceStatus InvoiceStatus { get; set; }

        /// <summary>Chỉ áp dụng cho TransactionFee: giá trị giao dịch gốc trước khi khấu trừ</summary>
        public decimal? OriginalTransactionValue { get; set; }
    }
}