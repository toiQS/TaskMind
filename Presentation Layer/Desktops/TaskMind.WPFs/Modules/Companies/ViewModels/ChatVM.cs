using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class ChatVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private ChatTab _currentTab = ChatTab.Active;
        /// <summary>Tab đang xem: Đang trao đổi hay Chờ xác nhận.</summary>
        public ChatTab CurrentTab
        {
            get => _currentTab;
            set { _currentTab = value; OnPropertyChanged(); ApplyFilter(); }
        }

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
                OnPropertyChanged(nameof(IsSelectedConversationPending));
                OnPropertyChanged(nameof(IsSelectedConversationActive));

                if (value != null && value.Status == ConversationStatus.Active && value.UnreadCount > 0)
                {
                    value.UnreadCount = 0;
                    RaiseCounters();
                }

                RefreshList();
            }
        }

        public bool HasSelectedConversation => SelectedConversation != null;
        public bool HasNoSelectedConversation => SelectedConversation == null;

        /// <summary>True khi hội thoại đang chọn còn ở trạng thái chờ xác nhận — dùng để ẩn khung chat, hiện thanh Chấp nhận/Từ chối.</summary>
        public bool IsSelectedConversationPending => SelectedConversation?.Status == ConversationStatus.PendingConfirmation;

        /// <summary>True khi hội thoại đang chọn đã được xác nhận — cho phép hiển thị khung chat bình thường.</summary>
        public bool IsSelectedConversationActive => SelectedConversation?.Status == ConversationStatus.Active;

        private string _messageInput;
        public string MessageInput { get => _messageInput; set { _messageInput = value; OnPropertyChanged(); } }

        /// <summary>Toàn bộ hội thoại tải từ service (cả đang trao đổi lẫn đang chờ xác nhận).</summary>
        public ObservableCollection<ConversationModel> Conversations { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tab hiện tại + tìm kiếm/lọc, dùng để bind lên View.</summary>
        public ObservableCollection<ConversationModel> FilteredConversations { get; } = new();

        /// <summary>Tập hội thoại theo tab hiện tại, trước khi áp dụng tìm kiếm/lọc loại.</summary>
        private IEnumerable<ConversationModel> ScopedConversations =>
            CurrentTab == ChatTab.Active
                ? Conversations.Where(c => c.Status == ConversationStatus.Active)
                : Conversations.Where(c => c.Status == ConversationStatus.PendingConfirmation);

        public int ActiveTabCount => Conversations.Count(c => c.Status == ConversationStatus.Active);
        public int PendingTabCount => Conversations.Count(c => c.Status == ConversationStatus.PendingConfirmation);

        /// <summary>Tổng tin chưa đọc — chỉ tính trong các hội thoại đã xác nhận (Active).</summary>
        public int TotalUnreadCount => Conversations.Where(c => c.Status == ConversationStatus.Active).Sum(c => c.UnreadCount);

        public ICommand RefreshCommand { get; }
        public ICommand SetTabCommand { get; }
        public ICommand OpenConversationCommand { get; }
        public ICommand CloseConversationCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand NewConversationCommand { get; }
        public ICommand AcceptRequestCommand { get; }
        public ICommand DeclineRequestCommand { get; }

        public ChatVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            SetTabCommand = new RelayCommand(p => CurrentTab = p is ChatTab t ? t : ChatTab.Active);
            OpenConversationCommand = new RelayCommand(p => SelectedConversation = p as ConversationModel);
            CloseConversationCommand = new RelayCommand(_ => SelectedConversation = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; TypeFilter = null; });
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is ConversationType t ? t : (ConversationType?)null);
            SendMessageCommand = new RelayCommand(_ => SendMessage(), _ => CanSendMessage());
            NewConversationCommand = new RelayCommand(_ => CreateConversation());
            AcceptRequestCommand = new RelayCommand(p => AcceptRequest(p as ConversationModel));
            DeclineRequestCommand = new RelayCommand(p => DeclineRequest(p as ConversationModel));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /chat/conversations thay cho dữ liệu mẫu bên dưới.
            // Backend nên trả kèm Status (Active/PendingConfirmation) để phân tách 2 tab.
            await Task.Delay(400);

            Conversations.Clear();

            // ================= TAB 1: ĐANG TRAO ĐỔI (đã xác nhận cả 2 bên) =================
            var direct = new ConversationModel
            {
                Name = "Lê Thị Hoa",
                Subtitle = "Technical Leader",
                Type = ConversationType.Direct,
                Status = ConversationStatus.Active,
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
                Status = ConversationStatus.Active,
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
                Type = ConversationType.Partner,
                Status = ConversationStatus.Active
            };
            partner.Messages.Add(new ChatMessageModel { SenderName = "ABC Corp", Content = "Bên mình cần thêm báo cáo tiến độ milestone 2.", SentTime = DateTime.Now.AddDays(-1) });
            partner.Messages.Add(new ChatMessageModel { SenderName = "Tôi", Content = "Dạ em gửi báo cáo trong hôm nay.", SentTime = DateTime.Now.AddDays(-1).AddHours(1), IsMine = true, Status = MessageStatus.Read });
            Conversations.Add(partner);

            var admin = new ConversationModel
            {
                Name = "Admin hệ thống",
                Subtitle = "Hỗ trợ kỹ thuật & tài khoản",
                Type = ConversationType.Admin,
                Status = ConversationStatus.Active,
                UnreadCount = 1
            };
            admin.Messages.Add(new ChatMessageModel { SenderName = "Tôi", Content = "Công ty em cần hỗ trợ gia hạn gói tham gia hệ thống.", SentTime = DateTime.Now.AddDays(-2), IsMine = true, Status = MessageStatus.Read });
            admin.Messages.Add(new ChatMessageModel { SenderName = "Admin hệ thống", Content = "Chào bạn, bên mình sẽ xử lý và phản hồi trong 24h.", SentTime = DateTime.Now.AddDays(-2).AddMinutes(30) });
            Conversations.Add(admin);

            // ================= TAB 2: MUỐN TRAO ĐỔI, CHỜ XÁC NHẬN =================
            Conversations.Add(new ConversationModel
            {
                Name = "XYZ Technology Co.",
                Subtitle = "Đối tác tiềm năng · Mobile Banking App",
                Type = ConversationType.Partner,
                Status = ConversationStatus.PendingConfirmation,
                RequestNote = "Chào bên mình, XYZ Technology muốn trao đổi hợp tác phát triển dự án Mobile Banking App theo hình thức trao đổi (mục 4.14), mong nhận được phản hồi.",
                RequestDate = DateTime.Now.AddHours(-3)
            });

            Conversations.Add(new ConversationModel
            {
                Name = "Trịnh Anh Tuấn",
                Subtitle = "Ứng viên tự do · Mobile Developer (Flutter/Firebase)",
                Type = ConversationType.Direct,
                Status = ConversationStatus.PendingConfirmation,
                RequestNote = "Chào anh/chị, em quan tâm tới dự án App quản lý kho (bảo trì) đang cần người, mong được trao đổi thêm ạ.",
                RequestDate = DateTime.Now.AddMinutes(-25)
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = ScopedConversations;

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    c.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.LastMessage?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.RequestNote?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (TypeFilter.HasValue)
                query = query.Where(c => c.Type == TypeFilter.Value);

            FilteredConversations.Clear();
            foreach (var c in query.OrderByDescending(c => c.IsPinned)
                                    .ThenByDescending(c => c.Status == ConversationStatus.Active ? c.LastMessageTime : c.RequestDate))
                FilteredConversations.Add(c);
        }

        private bool CanSendMessage()
            => SelectedConversation is { Status: ConversationStatus.Active } && !string.IsNullOrWhiteSpace(MessageInput);

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
            // gọi service POST /chat/conversations. Hội thoại mới với người/công ty ngoài hệ thống nội bộ
            // (đối tác, freelancer) nên khởi tạo Status = ConversationStatus.PendingConfirmation
            // để chờ bên còn lại xác nhận trước khi có thể nhắn tin qua lại.
        }

        /// <summary>Chấp nhận yêu cầu trao đổi: chuyển hội thoại sang tab "Đang trao đổi".</summary>
        private void AcceptRequest(ConversationModel conversation)
        {
            if (conversation == null || conversation.Status != ConversationStatus.PendingConfirmation) return;

            // TODO: gọi service POST /chat/conversations/{id}/accept
            conversation.Status = ConversationStatus.Active;
            conversation.Messages.Add(new ChatMessageModel
            {
                SenderName = "Hệ thống",
                Content = "Yêu cầu trao đổi đã được chấp nhận. Hai bên có thể bắt đầu trò chuyện.",
                SentTime = DateTime.Now
            });

            CurrentTab = ChatTab.Active; // ApplyFilter() đã được gọi trong setter của CurrentTab
            SelectedConversation = conversation;
            RaiseCounters();
        }

        /// <summary>Từ chối/huỷ yêu cầu trao đổi.</summary>
        private void DeclineRequest(ConversationModel conversation)
        {
            if (conversation == null || conversation.Status != ConversationStatus.PendingConfirmation) return;

            // TODO: gọi service POST /chat/conversations/{id}/decline
            if (ReferenceEquals(SelectedConversation, conversation))
                SelectedConversation = null;

            Conversations.Remove(conversation);
            ApplyFilter();
            RaiseCounters();
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
            OnPropertyChanged(nameof(ActiveTabCount));
            OnPropertyChanged(nameof(PendingTabCount));
        }
    }
}