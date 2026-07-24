using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class NotificationVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private NotificationType? _typeFilter;
        public NotificationType? TypeFilter { get => _typeFilter; set { _typeFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        /// <summary>null = tất cả, true = chỉ hiện thông báo chưa đọc, false = chỉ hiện đã đọc.</summary>
        private bool? _unreadOnlyFilter;
        public bool? UnreadOnlyFilter { get => _unreadOnlyFilter; set { _unreadOnlyFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private bool _isSettingsOpen;
        public bool IsSettingsOpen { get => _isSettingsOpen; set { _isSettingsOpen = value; OnPropertyChanged(); } }

        /// <summary>Toàn bộ thông báo tải từ service.</summary>
        public ObservableCollection<NotificationModel> Notifications { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc, dùng để bind lên View.</summary>
        public ObservableCollection<NotificationModel> FilteredNotifications { get; } = new();

        /// <summary>Tuỳ chọn kênh nhận theo từng loại thông báo (mục 5.3).</summary>
        public ObservableCollection<NotificationPreferenceModel> Preferences { get; } = new();

        public int UnreadCount => Notifications.Count(n => !n.IsRead);
        public bool HasUnread => UnreadCount > 0;
        public int TotalCount => Notifications.Count;

        public ICommand RefreshCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand SetUnreadOnlyCommand { get; }
        public ICommand OpenNotificationCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand SavePreferencesCommand { get; }

        public NotificationVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; TypeFilter = null; UnreadOnlyFilter = null; });
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is NotificationType t ? t : (NotificationType?)null);
            SetUnreadOnlyCommand = new RelayCommand(p => UnreadOnlyFilter = p switch
            {
                bool b => b,
                string s when bool.TryParse(s, out var parsed) => parsed,
                _ => (bool?)null
            });
            OpenNotificationCommand = new RelayCommand(p => OpenNotification(p as NotificationModel));
            MarkAsReadCommand = new RelayCommand(p => MarkAsRead(p as NotificationModel));
            MarkAllAsReadCommand = new RelayCommand(_ => MarkAllAsRead());
            DeleteCommand = new RelayCommand(p => Delete(p as NotificationModel));
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsOpen = !IsSettingsOpen);
            SavePreferencesCommand = new RelayCommand(async _ => await SavePreferencesAsync());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /notifications và GET /notifications/preferences thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Notifications.Clear();

            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.ProjectInvitation,
                Title = "Lời mời tham gia dự án",
                Message = "Bạn được mời tham gia dự án \"Website thương mại điện tử ABC\" với vai trò Developer.",
                RelatedEntityName = "Website thương mại điện tử ABC",
                CreatedDate = DateTime.Now.AddMinutes(-15),
                SentByEmail = true
            });
            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.TestResult,
                Title = "Có kết quả bài kiểm tra",
                Message = "Bài kiểm tra năng lực \"Backend Developer (.NET)\" đã có kết quả, điểm số 8.5/10.",
                RelatedEntityName = "Backend Developer (.NET)",
                CreatedDate = DateTime.Now.AddHours(-2),
                SentByEmail = true
            });
            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.ProfileApproval,
                Title = "Hồ sơ công ty đã được duyệt",
                Message = "Hồ sơ đăng ký công ty TaskMind Software JSC đã được Admin hệ thống xác thực.",
                RelatedEntityName = "TaskMind Software JSC",
                IsRead = true,
                CreatedDate = DateTime.Now.AddDays(-1),
                SentByEmail = true
            });
            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.Recruitment,
                Priority = NotificationPriority.Important,
                Title = "Ứng viên mới ứng tuyển",
                Message = "Nguyễn Văn A vừa nộp hồ sơ cho vị trí Backend Developer (.NET), mức khớp kỹ năng 88%.",
                RelatedEntityName = "Backend Developer (.NET)",
                CreatedDate = DateTime.Now.AddHours(-5)
            });
            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.Support,
                Title = "Yêu cầu hỗ trợ mới cần duyệt",
                Message = "Đề xuất nâng cấp RAM cho dàn máy dev đang chờ bạn duyệt.",
                RelatedEntityName = "Nâng cấp RAM cho dàn máy dev",
                CreatedDate = DateTime.Now.AddHours(-6)
            });
            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.CompanyInvitation,
                Title = "Lời mời gia nhập công ty",
                Message = "Bạn được mời gia nhập công ty TaskMind Software JSC với vai trò Staff.",
                RelatedEntityName = "TaskMind Software JSC",
                IsRead = true,
                CreatedDate = DateTime.Now.AddDays(-3)
            });
            Notifications.Add(new NotificationModel
            {
                Type = NotificationType.System,
                Priority = NotificationPriority.Important,
                Title = "Gói tham gia hệ thống sắp hết hạn",
                Message = "Gói Doanh nghiệp sẽ hết hạn trong 12 ngày, vui lòng gia hạn để không bị gián đoạn dịch vụ.",
                CreatedDate = DateTime.Now.AddDays(-4),
                SentByEmail = true
            });

            Preferences.Clear();
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.ProjectInvitation,
                DisplayName = "Mời tham gia dự án",
                Description = "Khi bạn được thêm vào một dự án mới."
            });
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.CompanyInvitation,
                DisplayName = "Mời tham gia công ty",
                Description = "Khi một công ty/cơ sở đào tạo mời bạn gia nhập."
            });
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.TestResult,
                DisplayName = "Kết quả bài kiểm tra",
                Description = "Khi có kết quả chấm bài kiểm tra năng lực/khoá học."
            });
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.ProfileApproval,
                DisplayName = "Phê duyệt hồ sơ",
                Description = "Khi hồ sơ công ty/cơ sở đào tạo được Admin xét duyệt."
            });
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.Recruitment,
                DisplayName = "Tuyển dụng & ứng viên",
                Description = "Ứng viên mới, cập nhật trạng thái ứng tuyển.",
                EmailEnabled = false
            });
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.Support,
                DisplayName = "Yêu cầu hỗ trợ nội bộ",
                Description = "Yêu cầu chờ duyệt hoặc đã được xử lý."
            });
            Preferences.Add(new NotificationPreferenceModel
            {
                Type = NotificationType.System,
                DisplayName = "Thông báo hệ thống chung",
                Description = "Gia hạn gói, bảo trì, thay đổi chính sách."
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Notifications.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(n =>
                    n.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    n.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (TypeFilter.HasValue)
                query = query.Where(n => n.Type == TypeFilter.Value);

            if (UnreadOnlyFilter.HasValue)
                query = query.Where(n => n.IsRead == !UnreadOnlyFilter.Value);

            FilteredNotifications.Clear();
            foreach (var n in query.OrderByDescending(n => n.Priority).ThenByDescending(n => n.CreatedDate))
                FilteredNotifications.Add(n);
        }

        /// <summary>Mở thông báo: điều hướng tới thực thể liên quan và tự động đánh dấu đã đọc.</summary>
        private void OpenNotification(NotificationModel notification)
        {
            if (notification == null) return;

            // TODO: điều hướng sang dự án/công ty/bài kiểm tra/tin tuyển dụng liên quan
            // dựa trên notification.Type + notification.RelatedEntityName.
            MarkAsRead(notification);
        }

        private void MarkAsRead(NotificationModel notification)
        {
            if (notification == null || notification.IsRead) return;

            // TODO: gọi service PATCH /notifications/{id}/read
            notification.IsRead = true;
            Touch();
        }

        private void MarkAllAsRead()
        {
            if (!HasUnread) return;

            // TODO: gọi service POST /notifications/mark-all-read
            foreach (var n in Notifications)
                n.IsRead = true;

            Touch();
        }

        private void Delete(NotificationModel notification)
        {
            if (notification == null) return;

            // TODO: gọi service DELETE /notifications/{id}
            Notifications.Remove(notification);
            Touch();
        }

        private async Task SavePreferencesAsync()
        {
            IsBusy = true;

            // TODO: gọi service PUT /notifications/preferences với danh sách Preferences hiện tại
            await Task.Delay(400);

            IsBusy = false;
            IsSettingsOpen = false;
        }

        /// <summary>Ép làm mới UI vì NotificationModel không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(UnreadCount));
            OnPropertyChanged(nameof(HasUnread));
            OnPropertyChanged(nameof(TotalCount));
        }
    }
}