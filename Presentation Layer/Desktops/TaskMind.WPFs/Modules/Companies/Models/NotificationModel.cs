using System;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>
    /// Loại thông báo hệ thống (mục 5.3): mời tham gia dự án/công ty, kết quả kiểm tra,
    /// phê duyệt hồ sơ, cùng các loại phát sinh từ nghiệp vụ tuyển dụng/hỗ trợ nội bộ.
    /// </summary>
    public enum NotificationType
    {
        ProjectInvitation,   // Mời tham gia dự án
        CompanyInvitation,   // Mời tham gia công ty (trở thành Staff)
        TestResult,          // Kết quả bài kiểm tra
        ProfileApproval,     // Phê duyệt hồ sơ công ty/cơ sở đào tạo
        Recruitment,         // Ứng viên mới / cập nhật tuyển dụng
        Support,             // Yêu cầu hỗ trợ nội bộ
        System               // Thông báo hệ thống chung (gói tham gia, bảo trì,...)
    }

    public enum NotificationPriority
    {
        Normal,
        Important
    }

    /// <summary>
    /// Một thông báo gửi tới người dùng qua kênh trong ứng dụng và/hoặc email (mục 5.3).
    /// </summary>
    public class NotificationModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public NotificationType Type { get; set; } = NotificationType.System;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public string Title { get; set; }
        public string Message { get; set; }

        /// <summary>Tên thực thể liên quan (dự án, công ty, bài kiểm tra...) để hiển thị ngữ cảnh nhanh và điều hướng.</summary>
        public string RelatedEntityName { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>Thông báo này có được gửi kèm qua email hay không (phụ thuộc NotificationPreferenceModel của người dùng).</summary>
        public bool SentByEmail { get; set; }
    }

    /// <summary>
    /// Tuỳ chọn nhận thông báo theo từng loại — bật/tắt kênh trong ứng dụng và email,
    /// cho phép người dùng cấu hình riêng (mục 5.3).
    /// </summary>
    public class NotificationPreferenceModel
    {
        public NotificationType Type { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public bool InAppEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = true;
    }
}