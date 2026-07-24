using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Loại hội thoại nội bộ: riêng (1-1) hoặc nhóm (nhiều thành viên cùng công ty).</summary>
    public enum ConversationType
    {
        Direct,
        Group
    }

    public enum MessageStatus
    {
        Sending,
        Sent,
        Delivered,
        Read
    }

    /// <summary>
    /// Chứa các hội thoại nội bộ công ty mà cá nhân đang tham gia, bao gồm hội thoại riêng (Direct)
    /// và hội thoại nhóm (Group). Khác với module Companies (trao đổi có thể gồm cả đối tác/khách
    /// hàng ngoài công ty), Chat ở đây chỉ diễn ra giữa các đồng nghiệp cùng công ty nên không cần
    /// trạng thái "chờ xác nhận" — hội thoại luôn sẵn sàng nhắn tin ngay khi được tạo.
    /// </summary>
    public class ConversationModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Tên người (Direct) hoặc tên nhóm (Group).</summary>
        public string Name { get; set; }

        /// <summary>Phụ đề: chức danh/phòng ban (Direct) hoặc mô tả ngắn (Group).</summary>
        public string Subtitle { get; set; }

        public ConversationType Type { get; set; } = ConversationType.Direct;

        /// <summary>Chỉ có ý nghĩa với Direct — trạng thái trực tuyến của người còn lại.</summary>
        public bool IsOnline { get; set; }

        /// <summary>Danh sách thành viên (chỉ có ý nghĩa với Group, bao gồm cả bản thân).</summary>
        public List<string> MemberNames { get; set; } = new();

        public bool IsPinned { get; set; }

        /// <summary>Dùng để tô sáng card hội thoại đang được chọn trong danh sách (không phải INotifyPropertyChanged).</summary>
        public bool IsSelected { get; set; }

        public int UnreadCount { get; set; }

        public ObservableCollection<ChatMessageModel> Messages { get; set; } = new();

        public string LastMessage => Messages.Count > 0 ? Messages[^1].Content : "Chưa có tin nhắn";
        public DateTime LastMessageTime => Messages.Count > 0 ? Messages[^1].SentTime : DateTime.MinValue;

        public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[0].ToString().ToUpper();

        public bool HasUnread => UnreadCount > 0;

        /// <summary>Dòng phụ hiển thị dưới tên hội thoại: chức danh (Direct) hoặc số thành viên (Group).</summary>
        public string MembersDisplay => Type == ConversationType.Group
            ? $"{MemberNames.Count} thành viên"
            : Subtitle;
    }

    public class ChatMessageModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentTime { get; set; } = DateTime.Now;

        /// <summary>True nếu tin nhắn do nhân sự hiện tại gửi (căn phải, đổi màu bong bóng).</summary>
        public bool IsMine { get; set; }

        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        public string Initial => string.IsNullOrWhiteSpace(SenderName) ? "?" : SenderName.Trim()[0].ToString().ToUpper();
    }

    /// <summary>Một đồng nghiệp trong công ty — dùng làm danh bạ khi bắt đầu hội thoại mới (Direct hoặc Group).</summary>
    public class ContactOption
    {
        public Guid StaffId { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public bool IsOnline { get; set; }

        /// <summary>True nếu đã được chọn trong picker "Bắt đầu hội thoại mới".</summary>
        public bool IsSelected { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
    }
}