using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>
    /// Loại hội thoại: trao đổi giữa nhân sự, theo dự án, với công ty đối tác (trao đổi mục 4.14),
    /// hoặc với Admin hệ thống khi cần hỗ trợ.
    /// </summary>
    public enum ConversationType
    {
        Direct,   // Trò chuyện riêng giữa 2 nhân sự
        Project,  // Trao đổi theo nhóm dự án
        Partner,  // Trao đổi với công ty/đối tác khác
        Admin     // Hỗ trợ từ Admin hệ thống
    }

    public enum MessageStatus
    {
        Sending,
        Sent,
        Delivered,
        Read
    }

    /// <summary>
    /// trao đổi giữa nhân sự, các cuộc hội thoại riêng trong dự án,
    /// với công ty khác và admin hệ thống khi cần thiết.
    /// </summary>
    public class ConversationModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Tên người/nhóm/công ty hiển thị trên đầu hội thoại.</summary>
        public string Name { get; set; }

        /// <summary>Phụ đề: tên dự án, tên công ty đối tác, vai trò, v.v.</summary>
        public string Subtitle { get; set; }

        public ConversationType Type { get; set; } = ConversationType.Direct;

        public bool IsOnline { get; set; }
        public bool IsPinned { get; set; }

        /// <summary>Dùng để tô sáng card hội thoại đang được chọn trong danh sách (không phải INotifyPropertyChanged).</summary>
        public bool IsSelected { get; set; }

        public int UnreadCount { get; set; }

        public ObservableCollection<ChatMessageModel> Messages { get; set; } = new();

        public string LastMessage => Messages.Count > 0 ? Messages[^1].Content : "Chưa có tin nhắn";
        public DateTime LastMessageTime => Messages.Count > 0 ? Messages[^1].SentTime : DateTime.MinValue;

        public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[0].ToString().ToUpper();

        public bool HasUnread => UnreadCount > 0;
    }

    public class ChatMessageModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentTime { get; set; } = DateTime.Now;

        /// <summary>True nếu tin nhắn do người dùng hiện tại gửi (căn phải, đổi màu bong bóng).</summary>
        public bool IsMine { get; set; }

        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        public string Initial => string.IsNullOrWhiteSpace(SenderName) ? "?" : SenderName.Trim()[0].ToString().ToUpper();
    }
}