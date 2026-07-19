using System;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Trạng thái kiểm duyệt/xác thực công ty bởi Admin hệ thống (mục 4.4).</summary>
    public enum CompanyVerificationStatus
    {
        PendingVerification, // Chờ duyệt
        Verified,            // Đã xác thực, hoạt động đầy đủ
        Rejected,            // Bị từ chối
        Suspended            // Bị tạm khoá
    }

    /// <summary>Chu kỳ thanh toán gói tham gia hệ thống.</summary>
    public enum BillingCycle
    {
        Monthly,
        Yearly
    }

    /// <summary>
    /// Thông tin công ty: đăng ký, khai báo pháp lý, lĩnh vực hoạt động (mục 4.4).
    /// Được kiểm duyệt bởi Admin hệ thống trước khi công ty được phép hoạt động đầy đủ.
    /// </summary>
    public class CompanyInfoModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ----- Thông tin cơ bản -----
        public string CompanyName { get; set; }
        public string TaxCode { get; set; }
        public string Industry { get; set; }
        public string CompanySize { get; set; }       // VD: "10-50 nhân viên"
        public DateTime? FoundedDate { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }

        // ----- Thông tin liên hệ & người đại diện -----
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string LegalRepresentativeName { get; set; }
        public string LegalRepresentativePosition { get; set; }

        // ----- Kiểm duyệt (mục 4.4) -----
        public CompanyVerificationStatus VerificationStatus { get; set; } = CompanyVerificationStatus.PendingVerification;
        public string VerificationNote { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? VerifiedDate { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(CompanyName) ? "?" : CompanyName.Trim()[0].ToString().ToUpper();
    }

    /// <summary>Gói tham gia hệ thống của công ty, thu phí định kỳ (subscription/membership, mục 4.4 + 4.13).</summary>
    public class MembershipPackageModel
    {
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMonths(1);
        public bool IsAutoRenew { get; set; }

        public int DaysRemaining => Math.Max(0, (ExpiryDate.Date - DateTime.Now.Date).Days);
        public bool IsExpiringSoon => DaysRemaining <= 7;

        public string PriceDisplay => BillingCycle == BillingCycle.Monthly
            ? $"{Price:N0} đ / tháng"
            : $"{Price:N0} đ / năm";
    }
}