using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class ChatVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private ConversationType? _typeFilter;
        public ConversationType? TypeFilter { get => _typeFilter; set { _typeFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private ConversationModel _selectedConversation;
        public ConversationModel SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                foreach (var c in Conversations)
                    c.IsSelected = ReferenceEquals(c, value);

                _selectedConversation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedConversation));
                OnPropertyChanged(nameof(HasNoSelectedConversation));

                if (value != null && value.UnreadCount > 0)
                {
                    value.UnreadCount = 0;
                    RaiseCounters();
                }

                RefreshList();
            }
        }

        public bool HasSelectedConversation => SelectedConversation != null;
        public bool HasNoSelectedConversation => SelectedConversation == null;

        private string _messageInput;
        public string MessageInput { get => _messageInput; set { _messageInput = value; OnPropertyChanged(); } }

        public ObservableCollection<ConversationModel> Conversations { get; } = new();
        public ObservableCollection<ConversationModel> FilteredConversations { get; } = new();

        public int TotalUnreadCount => Conversations.Sum(c => c.UnreadCount);

        public ICommand RefreshCommand { get; }
        public ICommand OpenConversationCommand { get; }
        public ICommand CloseConversationCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand NewConversationCommand { get; }

        public ChatVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            OpenConversationCommand = new RelayCommand(p => SelectedConversation = p as ConversationModel);
            CloseConversationCommand = new RelayCommand(_ => SelectedConversation = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; TypeFilter = null; });
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is ConversationType t ? t : (ConversationType?)null);
            SendMessageCommand = new RelayCommand(_ => SendMessage(), _ => CanSendMessage());
            NewConversationCommand = new RelayCommand(_ => CreateConversation());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /chat/conversations thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Conversations.Clear();

            var direct = new ConversationModel
            {
                Name = "Lê Thị Hoa",
                Subtitle = "Technical Leader",
                Type = ConversationType.Direct,
                IsOnline = true,
                UnreadCount = 2
            };
            direct.Messages.Add(new ChatMessageModel { SenderName = "Lê Thị Hoa", Content = "Anh xem giúp em PR module thanh toán nhé.", SentTime = DateTime.Now.AddMinutes(-40) });
            direct.Messages.Add(new ChatMessageModel { SenderName = "Tôi", Content = "Ok để anh review trong chiều nay.", SentTime = DateTime.Now.AddMinutes(-35), IsMine = true, Status = MessageStatus.Read });
            direct.Messages.Add(new ChatMessageModel { SenderName = "Lê Thị Hoa", Content = "Dạ em cảm ơn ạ.", SentTime = DateTime.Now.AddMinutes(-5) });
            Conversations.Add(direct);

            var project = new ConversationModel
            {
                Name = "Hệ thống ERP nội bộ",
                Subtitle = "Nhóm dự án · 6 thành viên",
                Type = ConversationType.Project,
                UnreadCount = 5
            };
            project.Messages.Add(new ChatMessageModel { SenderName = "Trần Văn Bình", Content = "Sprint tới mình ưu tiên module chấm công nhé cả nhà.", SentTime = DateTime.Now.AddHours(-3) });
            project.Messages.Add(new ChatMessageModel { SenderName = "Tôi", Content = "Em nhận phần API chấm công.", SentTime = DateTime.Now.AddHours(-2), IsMine = true, Status = MessageStatus.Delivered });
            project.Messages.Add(new ChatMessageModel { SenderName = "Đỗ Thu Trang", Content = "Em phụ trách viết test case cho phần đó.", SentTime = DateTime.Now.AddMinutes(-50) });
            Conversations.Add(project);

            var partner = new ConversationModel
            {
                Name = "ABC Corp",
                Subtitle = "Đối tác dự án trao đổi · Website TMĐT",
                Type = ConversationType.Partner
            };
            partner.Messages.Add(new ChatMessageModel { SenderName = "ABC Corp", Content = "Bên mình cần thêm báo cáo tiến độ milestone 2.", SentTime = DateTime.Now.AddDays(-1) });
            partner.Messages.Add(new ChatMessageModel { SenderName = "Tôi", Content = "Dạ em gửi báo cáo trong hôm nay.", SentTime = DateTime.Now.AddDays(-1).AddHours(1), IsMine = true, Status = MessageStatus.Read });
            Conversations.Add(partner);

            var admin = new ConversationModel
            {
                Name = "Admin hệ thống",
                Subtitle = "Hỗ trợ kỹ thuật & tài khoản",
                Type = ConversationType.Admin,
                UnreadCount = 1
            };
            admin.Messages.Add(new ChatMessageModel { SenderName = "Tôi", Content = "Công ty em cần hỗ trợ gia hạn gói tham gia hệ thống.", SentTime = DateTime.Now.AddDays(-2), IsMine = true, Status = MessageStatus.Read });
            admin.Messages.Add(new ChatMessageModel { SenderName = "Admin hệ thống", Content = "Chào bạn, bên mình sẽ xử lý và phản hồi trong 24h.", SentTime = DateTime.Now.AddDays(-2).AddMinutes(30) });
            Conversations.Add(admin);

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Conversations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    c.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.LastMessage?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (TypeFilter.HasValue)
                query = query.Where(c => c.Type == TypeFilter.Value);

            FilteredConversations.Clear();
            foreach (var c in query.OrderByDescending(c => c.IsPinned).ThenByDescending(c => c.LastMessageTime))
                FilteredConversations.Add(c);
        }

        private bool CanSendMessage()
            => SelectedConversation != null && !string.IsNullOrWhiteSpace(MessageInput);

        private void SendMessage()
        {
            if (!CanSendMessage()) return;

            // TODO: gọi service POST /chat/conversations/{id}/messages
            SelectedConversation.Messages.Add(new ChatMessageModel
            {
                SenderName = "Tôi",
                Content = MessageInput.Trim(),
                IsMine = true,
                Status = MessageStatus.Sent
            });

            MessageInput = string.Empty;

            OnPropertyChanged(nameof(SelectedConversation));
            ApplyFilter();
        }

        private void CreateConversation()
        {
            // TODO: mở dialog "Bắt đầu hội thoại mới" (chọn nhân sự / dự án / công ty đối tác / Admin),
            // gọi service POST /chat/conversations
        }

        /// <summary>Ép ItemsControl bên trái render lại container để DataTrigger đọc lại IsSelected mới nhất.</summary>
        private void RefreshList()
        {
            var current = FilteredConversations.ToList();
            FilteredConversations.Clear();
            foreach (var c in current)
                FilteredConversations.Add(c);
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(TotalUnreadCount));
        }
    }
}