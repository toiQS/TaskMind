using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum ChatPartnerType
    {
        Company,
        School,
        User
    }

    public class ConversationModel
    {
        public string Id { get; set; }
        public string PartnerName { get; set; }
        public ChatPartnerType PartnerType { get; set; }
        public bool IsOnline { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }

        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
    }

    public class MessageModel
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }

        /// <summary>true = Admin gửi (hiển thị bên phải), false = đối phương gửi (hiển thị bên trái)</summary>
        public bool IsMine { get; set; }
    }
}