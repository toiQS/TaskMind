using System;
using System.Collections.Generic;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Loại tin đăng: dự án thương mại (có thể tính phí trao đổi) hoặc mã nguồn mở (miễn phí, mục 4.12).</summary>
    public enum ListingType
    {
        Project,     // Dự án cũ cần bán/trao đổi (mục 4.14)
        OpenSource   // Dự án mã nguồn mở, không phát sinh phí giao dịch
    }

    /// <summary>Trạng thái kiểm duyệt & giao dịch của tin đăng.</summary>
    public enum ListingStatus
    {
        PendingApproval, // Chờ duyệt
        Published,       // Đã duyệt, hiển thị công khai
        Negotiating,      // Đang có bên trao đổi/thương lượng
        Sold,             // Đã bán/trao đổi thành công
        Rejected,         // Bị từ chối
        Closed            // Đã gỡ khỏi chợ
    }

    /// <summary>
    /// Tin đăng bán/trao đổi dự án cũ hoặc mã nguồn mở (mục 4.14 - Quản lý trao đổi).
    /// Dự án OpenSource không phát sinh phí giao dịch cho hệ thống (mục 4.12).
    /// </summary>
    public class StoreListingModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public ListingType Type { get; set; } = ListingType.Project;
        public ListingStatus Status { get; set; } = ListingStatus.PendingApproval;

        /// <summary>Danh sách công nghệ/kỹ năng liên quan, tham chiếu danh mục kỹ năng chuẩn hoá (mục 4.15).</summary>
        public List<string> TechStack { get; set; } = new();

        /// <summary>Giá trao đổi — null hoặc 0 với dự án OpenSource (miễn phí).</summary>
        public decimal? Price { get; set; }
        public bool IsNegotiable { get; set; }

        /// <summary>Cho phép thanh toán theo cột mốc thay vì trọn gói (liên kết mục 4.14).</summary>
        public bool MilestoneBasedPayment { get; set; }

        public string SellerName { get; set; }
        public string SellerCompany { get; set; }

        public string RepoUrl { get; set; }
        public string DemoUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int ViewCount { get; set; }
        public int InterestCount { get; set; }

        /// <summary>Lý do từ chối/ghi chú kiểm duyệt của Admin.</summary>
        public string AdminNote { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(SellerName) ? "?" : SellerName.Trim()[0].ToString().ToUpper();

        public string TechStackDisplay => TechStack is { Count: > 0 } ? string.Join(" · ", TechStack) : "Chưa gắn tag";
    }
}