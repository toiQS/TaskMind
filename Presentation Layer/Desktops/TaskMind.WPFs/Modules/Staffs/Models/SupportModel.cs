using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Trạng thái yêu cầu hỗ trợ, do Admin công ty cập nhật khi xử lý.</summary>
    public enum SupportStatus
    {
        Pending,     // Chờ xử lý
        InProgress,  // Đang xử lý
        Resolved,    // Đã giải quyết
        Closed       // Đã đóng (nhân sự tự đóng sau khi xác nhận đã giải quyết)
    }

    /// <summary>Nhóm nội dung yêu cầu hỗ trợ, giúp Admin công ty phân loại và điều phối xử lý.</summary>
    public enum SupportCategory
    {
        Account,    // Tài khoản / đăng nhập
        Technical,  // Kỹ thuật / hệ thống
        Salary,     // Lương thưởng / phúc lợi
        Project,    // Liên quan dự án
        Other       // Khác
    }

    /// <summary>Một tin nhắn trong luồng trao đổi của yêu cầu hỗ trợ — có thể do chính nhân sự
    /// gửi thêm (bổ sung thông tin) hoặc do Admin công ty phản hồi.</summary>
    public class SupportReplyModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>True nếu tin nhắn do chính nhân sự gửi, False nếu do Admin công ty phản hồi —
        /// dùng để tô màu khác nhau trong luồng trao đổi.</summary>
        public bool IsFromStaff { get; set; }
    }

    /// <summary>
    /// Yêu cầu hỗ trợ cá nhân của nhân sự gửi đến Admin công ty. Nhân sự chỉ được tạo mới và bổ
    /// sung thông tin (reply) khi yêu cầu chưa đóng; trạng thái Pending/InProgress/Resolved do
    /// Admin công ty cập nhật, riêng bước Resolved -> Closed do chính nhân sự xác nhận.
    /// </summary>
    public class SupportRequestModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public SupportCategory Category { get; set; } = SupportCategory.Other;
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;
        public SupportStatus Status { get; set; } = SupportStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ResolvedDate { get; set; }

        /// <summary>Tên Admin công ty đang thụ lý — null nếu chưa được ai nhận xử lý.</summary>
        public string AdminName { get; set; }

        public ObservableCollection<SupportReplyModel> Replies { get; set; } = new();

        /// <summary>True nếu yêu cầu vẫn còn mở (còn có thể bổ sung thông tin).</summary>
        public bool IsOpen => Status is SupportStatus.Pending or SupportStatus.InProgress or SupportStatus.Resolved;

        /// <summary>Chỉ cho phép nhân sự tự đóng khi Admin đã đánh dấu Resolved.</summary>
        public bool CanClose => Status == SupportStatus.Resolved;

        public int ReplyCount => Replies?.Count ?? 0;

        public DateTime LastActivityDate => Replies != null && Replies.Count > 0
            ? Replies.Max(r => r.CreatedDate)
            : CreatedDate;
    }
}