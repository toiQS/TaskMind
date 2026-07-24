using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class ChatVM : ViewModelBase
    {
        // TODO: thay bằng tên nhân sự đang đăng nhập lấy từ phiên làm việc thực tế.
        private const string CurrentUserName = "Lê Thị Hoa";

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

        /// <summary>Toàn bộ hội thoại nội bộ (Direct + Group) mà nhân sự hiện tại đang tham gia.</summary>
        public ObservableCollection<ConversationModel> Conversations { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc loại, dùng để bind lên View.</summary>
        public ObservableCollection<ConversationModel> FilteredConversations { get; } = new();

        public int TotalUnreadCount => Conversations.Sum(c => c.UnreadCount);
        public int DirectCount => Conversations.Count(c => c.Type == ConversationType.Direct);
        public int GroupCount => Conversations.Count(c => c.Type == ConversationType.Group);

        // ===================== Bắt đầu hội thoại mới =====================

        /// <summary>True khi panel "Bắt đầu hội thoại mới" đang mở (overlay ở ChatView).</summary>
        private bool _isStartingNew;
        public bool IsStartingNew { get => _isStartingNew; set { _isStartingNew = value; OnPropertyChanged(); } }

        private string _contactSearchText;
        public string ContactSearchText { get => _contactSearchText; set { _contactSearchText = value; OnPropertyChanged(); ApplyContactFilter(); } }

        /// <summary>Chỉ cần nhập khi chọn từ 2 đồng nghiệp trở lên (tạo nhóm).</summary>
        private string _groupNameInput;
        public string GroupNameInput { get => _groupNameInput; set { _groupNameInput = value; OnPropertyChanged(); } }

        private string _newConversationError;
        public string NewConversationError { get => _newConversationError; set { _newConversationError = value; OnPropertyChanged(); } }

        /// <summary>Danh bạ đồng nghiệp cùng công ty, dùng để chọn người bắt đầu hội thoại mới.</summary>
        public ObservableCollection<ContactOption> AvailableContacts { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm trong picker.</summary>
        public ObservableCollection<ContactOption> FilteredContacts { get; } = new();

        public int SelectedContactCount => AvailableContacts.Count(c => c.IsSelected);

        /// <summary>Chỉ bắt buộc đặt tên nhóm khi chọn từ 2 người trở lên; 1 người thì tạo hội thoại riêng.</summary>
        public bool RequiresGroupName => SelectedContactCount > 1;

        public ICommand RefreshCommand { get; }
        public ICommand OpenConversationCommand { get; }
        public ICommand CloseConversationCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand SendMessageCommand { get; }

        public ICommand OpenNewConversationCommand { get; }
        public ICommand CloseNewConversationCommand { get; }
        public ICommand ToggleContactCommand { get; }
        public ICommand StartConversationCommand { get; }

        public ChatVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            OpenConversationCommand = new RelayCommand(p => SelectedConversation = p as ConversationModel);
            CloseConversationCommand = new RelayCommand(_ => SelectedConversation = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; TypeFilter = null; });
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is ConversationType t ? t : (ConversationType?)null);
            SendMessageCommand = new RelayCommand(_ => SendMessage(), _ => CanSendMessage());

            OpenNewConversationCommand = new RelayCommand(_ => OpenNewConversation());
            CloseNewConversationCommand = new RelayCommand(_ => IsStartingNew = false);
            ToggleContactCommand = new RelayCommand(p => ToggleContact(p as ContactOption));
            StartConversationCommand = new RelayCommand(_ => StartConversation());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/conversations (hội thoại nội bộ đang tham gia) và
            // GET /company/{companyId}/staffs (danh bạ đồng nghiệp) thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            Conversations.Clear();

            var direct1 = new ConversationModel
            {
                Name = "Trần Văn Bình",
                Subtitle = "Project Manager",
                Type = ConversationType.Direct,
                IsOnline = true,
                UnreadCount = 2
            };
            direct1.Messages.Add(new ChatMessageModel { SenderName = "Trần Văn Bình", Content = "Em xem giúp anh tiến độ module chấm công nhé.", SentTime = DateTime.Now.AddMinutes(-40) });
            direct1.Messages.Add(new ChatMessageModel { SenderName = CurrentUserName, Content = "Dạ em đang làm, chiều nay xong ạ.", SentTime = DateTime.Now.AddMinutes(-35), IsMine = true, Status = MessageStatus.Read });
            direct1.Messages.Add(new ChatMessageModel { SenderName = "Trần Văn Bình", Content = "Ok cảm ơn em.", SentTime = DateTime.Now.AddMinutes(-5) });
            Conversations.Add(direct1);

            var direct2 = new ConversationModel
            {
                Name = "Đỗ Thu Trang",
                Subtitle = "Backend Developer",
                Type = ConversationType.Direct,
                IsOnline = false
            };
            direct2.Messages.Add(new ChatMessageModel { SenderName = CurrentUserName, Content = "Chị ơi review giúp em PR API báo cáo với ạ.", SentTime = DateTime.Now.AddHours(-3), IsMine = true, Status = MessageStatus.Delivered });
            Conversations.Add(direct2);

            var group1 = new ConversationModel
            {
                Name = "Team ERP nội bộ",
                Subtitle = "Nhóm trao đổi tiến độ dự án ERP",
                Type = ConversationType.Group,
                MemberNames = new() { CurrentUserName, "Trần Văn Bình", "Nguyễn Văn A" },
                UnreadCount = 5
            };
            group1.Messages.Add(new ChatMessageModel { SenderName = "Trần Văn Bình", Content = "Sprint tới ưu tiên module chấm công nhé cả nhà.", SentTime = DateTime.Now.AddHours(-2) });
            group1.Messages.Add(new ChatMessageModel { SenderName = CurrentUserName, Content = "Em nhận phần API chấm công.", SentTime = DateTime.Now.AddHours(-1), IsMine = true, Status = MessageStatus.Delivered });
            group1.Messages.Add(new ChatMessageModel { SenderName = "Nguyễn Văn A", Content = "Em phụ trách viết test case cho phần đó.", SentTime = DateTime.Now.AddMinutes(-30) });
            Conversations.Add(group1);

            var group2 = new ConversationModel
            {
                Name = "Phòng Kỹ thuật",
                Subtitle = "Kênh chung của phòng ban",
                Type = ConversationType.Group,
                MemberNames = new() { CurrentUserName, "Trần Văn Bình", "Đỗ Thu Trang", "Nguyễn Văn A", "Phạm Minh Tuấn" }
            };
            group2.Messages.Add(new ChatMessageModel { SenderName = "Phạm Minh Tuấn", Content = "Thứ 6 này phòng mình họp offline lúc 3h chiều nhé.", SentTime = DateTime.Now.AddDays(-1) });
            Conversations.Add(group2);

            AvailableContacts.Clear();
            AvailableContacts.Add(new ContactOption { FullName = "Trần Văn Bình", Position = "Project Manager", Department = "Phòng Kỹ thuật", IsOnline = true });
            AvailableContacts.Add(new ContactOption { FullName = "Đỗ Thu Trang", Position = "Backend Developer", Department = "Phòng Kỹ thuật", IsOnline = false });
            AvailableContacts.Add(new ContactOption { FullName = "Nguyễn Văn A", Position = "Developer", Department = "Phòng Kỹ thuật", IsOnline = true });
            AvailableContacts.Add(new ContactOption { FullName = "Phạm Minh Tuấn", Position = "Project Manager", Department = "Phòng Kỹ thuật", IsOnline = false });
            AvailableContacts.Add(new ContactOption { FullName = "Vũ Thị Mai", Position = "Project Manager", Department = "Ban Điều hành", IsOnline = true });

            ApplyFilter();
            ApplyContactFilter();
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
                SenderName = CurrentUserName,
                Content = MessageInput.Trim(),
                IsMine = true,
                Status = MessageStatus.Sent
            });

            MessageInput = string.Empty;

            OnPropertyChanged(nameof(SelectedConversation));
            ApplyFilter();
        }

        // ===================== Bắt đầu hội thoại mới =====================

        private void OpenNewConversation()
        {
            ContactSearchText = string.Empty;
            GroupNameInput = string.Empty;
            NewConversationError = string.Empty;

            foreach (var c in AvailableContacts)
                c.IsSelected = false;

            ApplyContactFilter();
            OnPropertyChanged(nameof(SelectedContactCount));
            OnPropertyChanged(nameof(RequiresGroupName));

            IsStartingNew = true;
        }

        private void ApplyContactFilter()
        {
            var query = AvailableContacts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ContactSearchText))
                query = query.Where(c =>
                    c.FullName?.Contains(ContactSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.Position?.Contains(ContactSearchText, StringComparison.OrdinalIgnoreCase) == true);

            FilteredContacts.Clear();
            foreach (var c in query.OrderBy(c => c.FullName))
                FilteredContacts.Add(c);
        }

        /// <summary>Bấm vào 1 đồng nghiệp trong picker: chọn/bỏ chọn để đưa vào hội thoại sắp tạo.</summary>
        private void ToggleContact(ContactOption contact)
        {
            if (contact == null) return;

            contact.IsSelected = !contact.IsSelected;

            OnPropertyChanged(nameof(SelectedContactCount));
            OnPropertyChanged(nameof(RequiresGroupName));

            RefreshContactList();
        }

        /// <summary>Tạo hội thoại mới: 1 người → hội thoại riêng (mở lại nếu đã tồn tại), từ 2 người
        /// trở lên → hội thoại nhóm với tên do người dùng đặt.</summary>
        private void StartConversation()
        {
            var selected = AvailableContacts.Where(c => c.IsSelected).ToList();

            if (selected.Count == 0)
            {
                NewConversationError = "Vui lòng chọn ít nhất 1 đồng nghiệp.";
                return;
            }

            if (selected.Count == 1)
            {
                var contact = selected[0];

                // Nếu đã có hội thoại riêng với người này thì mở lại thay vì tạo trùng.
                var existing = Conversations.FirstOrDefault(c =>
                    c.Type == ConversationType.Direct &&
                    string.Equals(c.Name, contact.FullName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    IsStartingNew = false;
                    SelectedConversation = existing;
                    return;
                }

                var direct = new ConversationModel
                {
                    Name = contact.FullName,
                    Subtitle = contact.Position,
                    Type = ConversationType.Direct,
                    IsOnline = contact.IsOnline
                };

                // TODO: gọi service POST /chat/conversations (kind = Direct)
                Conversations.Insert(0, direct);
                ApplyFilter();
                RaiseCounters();

                IsStartingNew = false;
                SelectedConversation = direct;
                return;
            }

            if (string.IsNullOrWhiteSpace(GroupNameInput))
            {
                NewConversationError = "Vui lòng đặt tên cho nhóm.";
                return;
            }

            var group = new ConversationModel
            {
                Name = GroupNameInput.Trim(),
                Subtitle = "Nhóm chat nội bộ",
                Type = ConversationType.Group,
                MemberNames = new List<string> { CurrentUserName }.Concat(selected.Select(c => c.FullName)).ToList()
            };

            // TODO: gọi service POST /chat/conversations (kind = Group)
            Conversations.Insert(0, group);
            ApplyFilter();
            RaiseCounters();

            IsStartingNew = false;
            SelectedConversation = group;
        }

        /// <summary>Ép ItemsControl bên trái render lại container để DataTrigger đọc lại IsSelected mới nhất
        /// (ConversationModel không implement INotifyPropertyChanged).</summary>
        private void RefreshList()
        {
            var current = FilteredConversations.ToList();
            FilteredConversations.Clear();
            foreach (var c in current)
                FilteredConversations.Add(c);
        }

        /// <summary>Ép ItemsControl trong picker render lại container để DataTrigger đọc IsSelected mới nhất.</summary>
        private void RefreshContactList()
        {
            var current = FilteredContacts.ToList();
            FilteredContacts.Clear();
            foreach (var c in current)
                FilteredContacts.Add(c);
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(TotalUnreadCount));
            OnPropertyChanged(nameof(DirectCount));
            OnPropertyChanged(nameof(GroupCount));
        }
    }
}