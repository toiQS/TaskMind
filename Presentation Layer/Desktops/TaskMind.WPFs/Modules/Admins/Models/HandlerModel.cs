using System;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum ReportStatus
    {
        Pending,     // Chờ xử lý
        Reviewing,   // Đang xác minh
        Resolved,    // Đã xử lý
        Dismissed    // Đã bỏ qua (không vi phạm)
    }

    public enum ReportPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum ViolationType
    {
        SpamContent,          // Nội dung spam
        Harassment,           // Quấy rối/xúc phạm
        FraudPayment,         // Gian lận thanh toán
        FakeInformation,      // Thông tin giả mạo
        IntellectualProperty, // Vi phạm bản quyền
        Other
    }

    public enum ReportedEntityType
    {
        User,
        Company,
        School,
        Project
    }

    public enum ResolutionAction
    {
        Warning,             // Cảnh cáo
        LockAccount,         // Khoá tài khoản
        BanAccount,          // Cấm tài khoản
        SuspendOrganization, // Tạm ngưng công ty/cơ sở đào tạo
        Dismiss,             // Bỏ qua, không vi phạm
        Other
    }

    public class ReportModel
    {
        public string Id { get; set; }
        public string ReporterName { get; set; }

        public string ReportedEntityId { get; set; }
        public string ReportedEntityName { get; set; }
        public ReportedEntityType ReportedEntityType { get; set; }

        public ViolationType ViolationType { get; set; }
        public ReportPriority Priority { get; set; }
        public string Description { get; set; }

        public ReportStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public ResolutionModel Resolution { get; set; }
    }

    public class ResolutionModel
    {
        public ResolutionAction Action { get; set; }
        public string Note { get; set; }
        public string ResolvedBy { get; set; }
        public DateTime ResolvedDate { get; set; }
    }

    /// <summary>Một dòng nhật ký kiểm toán, tra theo EntityId để xác minh vi phạm</summary>
    public class AuditLogEntryModel
    {
        public string Id { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }
}