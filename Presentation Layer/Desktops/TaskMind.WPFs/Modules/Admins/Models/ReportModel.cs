using System;
using System.Collections.Generic;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    /// <summary>
    /// Nguồn thu của hệ thống (mục 4.13 - Quản lý lợi nhuận):
    /// - Phí khấu trừ trên giao dịch trao đổi dự án
    /// - Phí tham gia hệ thống định kỳ từ Công ty / Cơ sở đào tạo
    /// </summary>
    public enum RevenueSourceType
    {
        TransactionFee,        // Phí khấu trừ giao dịch trao đổi
        CompanySubscription,   // Phí tham gia của công ty
        SchoolSubscription     // Phí tham gia của cơ sở đào tạo
    }

    /// <summary>Số liệu tổng quan cho kỳ báo cáo đang chọn.</summary>
    public class ReportSummary
    {
        public decimal TotalRevenue { get; set; }

        /// <summary>% tăng/giảm so với kỳ trước, ví dụ 12.5 nghĩa là +12.5%</summary>
        public double RevenueDeltaPercent { get; set; }

        public decimal TransactionFeeRevenue { get; set; }
        public decimal SubscriptionRevenue { get; set; }
        public int TotalTransactions { get; set; }
    }

    /// <summary>Một điểm dữ liệu trên biểu đồ xu hướng doanh thu.</summary>
    public class RevenuePoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }

    /// <summary>Tỉ trọng doanh thu theo từng nguồn thu, dùng để vẽ progress bar breakdown.</summary>
    public class RevenueBySourceItem
    {
        public RevenueSourceType Source { get; set; }
        public decimal Amount { get; set; }

        /// <summary>0 - 100</summary>
        public double Percentage { get; set; }
    }

    /// <summary>Một dòng giao dịch/đối soát gần đây.</summary>
    public class TransactionRecord
    {
        public string Id { get; set; }

        /// <summary>Tên công ty / cơ sở đào tạo / cặp trao đổi liên quan</summary>
        public string PartnerName { get; set; }

        public RevenueSourceType Source { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}