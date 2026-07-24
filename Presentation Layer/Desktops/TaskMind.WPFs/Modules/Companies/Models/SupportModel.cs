namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Loại yêu cầu hỗ trợ nội bộ công ty.</summary>
    public enum SupportType
    {
        DeviceUpgrade,      // Nâng cấp thiết bị
        Hiring,              // Tuyển thêm nhân sự
        EnvironmentUpdate,   // Yêu cầu cập nhật môi trường
        LicensePurchase,     // Mua bản quyền
        Other                // Khác
    }

    public enum SupportStatus
    {
        Pending,     // Chờ duyệt
        Approved,    // Đã duyệt
        InProgress,  // Đang xử lý
        Completed,   // Hoàn tất
        Rejected     // Từ chối
    }

    public enum SupportPriority
    {
        Low, Medium, High, Urgent
    }

    /// <summary>
    /// Yêu cầu hỗ trợ nội bộ: nâng cấp thiết bị, tuyển thêm nhân sự,
    /// yêu cầu cập nhật môi trường, mua bản quyền,...
    /// </summary>
    public class SupportRequestModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public SupportType Type { get; set; } = SupportType.Other;
        public SupportStatus Status { get; set; } = SupportStatus.Pending;
        public SupportPriority Priority { get; set; } = SupportPriority.Medium;

        public string RequestedBy { get; set; }
        public string Department { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ResolvedDate { get; set; }

        /// <summary>Chi phí ước tính (nâng cấp thiết bị / mua bản quyền...).</summary>
        public decimal? EstimatedCost { get; set; }

        public string AdminResponse { get; set; }

        /// <summary>Dùng cho avatar tròn — tránh lỗi Name[0] binding trực tiếp trong XAML.</summary>
        public string Initial => string.IsNullOrWhiteSpace(RequestedBy)
            ? "?" : RequestedBy.Trim()[0].ToString().ToUpper();
    }
}