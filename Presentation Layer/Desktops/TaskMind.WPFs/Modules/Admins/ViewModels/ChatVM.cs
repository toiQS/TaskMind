using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class ChatVM : ViewModelBase
    {
        public ObservableCollection<ConversationModel> Conversations { get; } = new ObservableCollection<ConversationModel>();

        private ICollectionView _conversationsView;
        public ICollectionView ConversationsView
        {
            get => _conversationsView;
            private set { _conversationsView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ConversationsView?.Refresh(); }
        }

        private ConversationModel _selectedConversation;
        public ConversationModel SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                _selectedConversation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedConversation));

                if (value != null && value.UnreadCount > 0)
                {
                    value.UnreadCount = 0;
                    Touch(value);
                }
            }
        }

        public bool HasSelectedConversation => SelectedConversation != null;

        private string _newMessageText;
        public string NewMessageText
        {
            get => _newMessageText;
            set { _newMessageText = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand SendMessageCommand { get; }

        public ChatVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            SendMessageCommand = new RelayCommand(_ => SendMessage());

            ConversationsView = CollectionViewSource.GetDefaultView(Conversations);
            ConversationsView.Filter = FilterConversations;

            _ = LoadDataAsync();
        }

        private bool FilterConversations(object obj)
        {
            if (obj is not ConversationModel c) return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                c.PartnerName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void SendMessage()
        {
            if (SelectedConversation == null || string.IsNullOrWhiteSpace(NewMessageText))
                return;

            var message = new MessageModel
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Content = NewMessageText.Trim(),
                SentAt = DateTime.Now,
                IsMine = true
            };

            // TODO: gọi service gửi tin nhắn thực tế (POST /conversations/{id}/messages)
            SelectedConversation.Messages.Add(message);
            SelectedConversation.LastMessage = message.Content;
            SelectedConversation.LastMessageTime = message.SentAt;

            NewMessageText = string.Empty;
        }

        /// <summary>ConversationModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item để UI cập nhật badge.</summary>
        private void Touch(ConversationModel changed)
        {
            int index = Conversations.IndexOf(changed);
            if (index >= 0)
            {
                Conversations.RemoveAt(index);
                Conversations.Insert(index, changed);
                SelectedConversation = changed;
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy danh sách hội thoại hỗ trợ.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Conversations.Clear();

            var c1 = new ConversationModel
            {
                Id = "CV001",
                PartnerName = "FPT Software",
                PartnerType = ChatPartnerType.Company,
                IsOnline = true,
                UnreadCount = 2,
            };
            c1.Messages.Add(new MessageModel { Id = "M1", Content = "Chào Admin, chúng tôi cần hỗ trợ nâng cấp gói Enterprise.", SentAt = DateTime.Now.AddMinutes(-40), IsMine = false });
            c1.Messages.Add(new MessageModel { Id = "M2", Content = "Chào anh/chị, TaskMind đã nhận được yêu cầu, sẽ phản hồi trong 24h.", SentAt = DateTime.Now.AddMinutes(-35), IsMine = true });
            c1.Messages.Add(new MessageModel { Id = "M3", Content = "Cảm ơn Admin, khi nào có kết quả nhờ báo giúp qua email nhé.", SentAt = DateTime.Now.AddMinutes(-10), IsMine = false });
            c1.LastMessage = c1.Messages[^1].Content;
            c1.LastMessageTime = c1.Messages[^1].SentAt;

            var c2 = new ConversationModel
            {
                Id = "CV002",
                PartnerName = "FUNiX Academy",
                PartnerType = ChatPartnerType.School,
                IsOnline = false,
                UnreadCount = 0,
            };
            c2.Messages.Add(new MessageModel { Id = "M4", Content = "Kỹ năng Rust chúng tôi đề xuất đã được duyệt chưa ạ?", SentAt = DateTime.Now.AddHours(-3), IsMine = false });
            c2.Messages.Add(new MessageModel { Id = "M5", Content = "Đã duyệt rồi anh/chị nhé, cảm ơn đã đóng góp cho danh mục kỹ năng.", SentAt = DateTime.Now.AddHours(-2), IsMine = true });
            c2.LastMessage = c2.Messages[^1].Content;
            c2.LastMessageTime = c2.Messages[^1].SentAt;

            var c3 = new ConversationModel
            {
                Id = "CV003",
                PartnerName = "Vũ Đức Anh",
                PartnerType = ChatPartnerType.User,
                IsOnline = false,
                UnreadCount = 1,
            };
            c3.Messages.Add(new MessageModel { Id = "M6", Content = "Tại sao tài khoản của tôi bị cấm? Tôi muốn khiếu nại.", SentAt = DateTime.Now.AddDays(-1), IsMine = false });
            c3.LastMessage = c3.Messages[^1].Content;
            c3.LastMessageTime = c3.Messages[^1].SentAt;

            foreach (var c in new[] { c1, c2, c3 })
                Conversations.Add(c);

            SelectedConversation = Conversations.FirstOrDefault();

            IsBusy = false;
        }
    }
}