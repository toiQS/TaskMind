using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Loại thông báo hệ thống (mục 5.3 - Quản lý thông báo): mời tham gia dự án/công ty,
    /// kết quả kiểm tra, phê duyệt hồ sơ, cùng các loại phát sinh từ nghiệp vụ khác của module Staffs
    /// (giao việc, hỗ trợ, trò chuyện) để bảng thông báo trở thành điểm tổng hợp chung.</summary>
    public enum NotificationType
    {
        ProjectInvite,    // Mời tham gia dự án
        TaskAssigned,     // Được giao công việc mới (mục 4.7)
        TestResult,       // Kết quả bài kiểm tra (mục 4.6/4.10)
        ProfileApproval,  // Phê duyệt hồ sơ / nâng cấp vai trò
        Support,          // Admin phản hồi yêu cầu hỗ trợ (liên kết SupportModel)
        Chat,             // Có tin nhắn nội bộ mới
        System            // Thông báo chung của hệ thống
    }

    /// <summary>
    /// Một thông báo cá nhân — hiển thị đồng thời trong bảng thông báo nhanh (bell dropdown ở
    /// StaffPage) và trang "Tất cả thông báo" (NotificationView). Kế thừa ViewModelBase để đánh dấu
    /// đã đọc cập nhật UI ngay lập tức trên cả 2 nơi hiển thị mà không cần ép làm mới cả danh sách.
    /// </summary>
    public class NotificationItemModel : ViewModelBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public NotificationType Type { get; set; } = NotificationType.System;
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>Tên thực thể liên quan (tên dự án, tên yêu cầu hỗ trợ...) — hiển thị phụ, có thể để trống.</summary>
        public string RelatedName { get; set; }

        private bool _isRead;
        public bool IsRead
        {
            get => _isRead;
            set { _isRead = value; OnPropertyChanged(); }
        }
    }
}