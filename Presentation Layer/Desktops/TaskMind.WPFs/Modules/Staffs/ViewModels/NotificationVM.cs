using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    /// <summary>
    /// ViewModel Quản lý thông báo (mục 5.3): phục vụ đồng thời bảng thông báo nhanh (bell dropdown
    /// tại StaffPage) và trang "Tất cả thông báo" (NotificationView). StaffNavigationVM giữ MỘT
    /// instance duy nhất xuyên suốt phiên làm việc để không mất badge/trạng thái đã đọc khi người
    /// dùng chuyển qua lại các module khác (khác các *VM khác trong Staffs vốn tạo mới mỗi lần bấm menu).
    /// </summary>
    public class NotificationVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        /// <summary>True khi bảng thông báo nhanh (dropdown cạnh chuông) đang mở.</summary>
        private bool _isPanelOpen;
        public bool IsPanelOpen { get => _isPanelOpen; set { _isPanelOpen = value; OnPropertyChanged(); } }

        private NotificationType? _typeFilter;
        public NotificationType? TypeFilter { get => _typeFilter; set { _typeFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private bool _unreadOnly;
        public bool UnreadOnly { get => _unreadOnly; set { _unreadOnly = value; OnPropertyChanged(); ApplyFilter(); } }

        /// <summary>Toàn bộ thông báo cá nhân, tải từ service.</summary>
        public ObservableCollection<NotificationItemModel> Notifications { get; } = new();

        /// <summary>Danh sách sau khi áp dụng lọc — dùng cho trang "Tất cả thông báo".</summary>
        public ObservableCollection<NotificationItemModel> FilteredNotifications { get; } = new();

        /// <summary>5 thông báo gần nhất — dùng cho bảng thông báo nhanh (bell dropdown).</summary>
        public ObservableCollection<NotificationItemModel> RecentNotifications { get; } = new();

        public int TotalCount => Notifications.Count;
        public int UnreadCount => Notifications.Count(n => !n.IsRead);

        public ICommand RefreshCommand { get; }
        public ICommand TogglePanelCommand { get; }
        public ICommand ClosePanelCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand ToggleUnreadOnlyCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DeleteAllCommand { get; }

        public NotificationVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            TogglePanelCommand = new RelayCommand(_ => IsPanelOpen = !IsPanelOpen);
            ClosePanelCommand = new RelayCommand(_ => IsPanelOpen = false);
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is NotificationType t ? t : (NotificationType?)null);
            ToggleUnreadOnlyCommand = new RelayCommand(_ => UnreadOnly = !UnreadOnly);
            ClearFilterCommand = new RelayCommand(_ => { TypeFilter = null; UnreadOnly = false; });
            MarkAsReadCommand = new RelayCommand(p => MarkAsRead(p as NotificationItemModel));
            MarkAllAsReadCommand = new RelayCommand(_ => MarkAllAsRead(), _ => UnreadCount > 0);
            DeleteCommand = new RelayCommand(p => Delete(p as NotificationItemModel));
            DeleteAllCommand = new RelayCommand(_ => DeleteAll(), _ => Notifications.Count > 0);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/notifications thay cho dữ liệu mẫu bên dưới (mục 5.3).
            await Task.Delay(300);

            Notifications.Clear();

            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.ProjectInvite,
                Title = "Lời mời tham gia dự án mới",
                Content = "Bạn được mời tham gia dự án \"Nền tảng học trực tuyến\" với vai trò Developer.",
                RelatedName = "Nền tảng học trực tuyến",
                CreatedDate = DateTime.Now.AddMinutes(-15)
            });
            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.TaskAssigned,
                Title = "Bạn vừa được giao công việc mới",
                Content = "Trần Văn Bình đã giao cho bạn công việc \"Thiết kế API chấm công\", hạn hoàn thành trong 2 ngày.",
                RelatedName = "Hệ thống ERP nội bộ",
                CreatedDate = DateTime.Now.AddHours(-1)
            });
            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.TestResult,
                Title = "Có kết quả bài kiểm tra",
                Content = "Kết quả bài kiểm tra năng lực định kỳ của bạn đã được công bố.",
                CreatedDate = DateTime.Now.AddHours(-3),
                IsRead = true
            });
            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.ProfileApproval,
                Title = "Hồ sơ đã được phê duyệt",
                Content = "Admin công ty đã phê duyệt cập nhật hồ sơ cá nhân của bạn.",
                CreatedDate = DateTime.Now.AddHours(-6),
                IsRead = true
            });
            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.Support,
                Title = "Admin đã phản hồi yêu cầu hỗ trợ",
                Content = "Yêu cầu \"Không đăng nhập được sau khi đổi mật khẩu\" vừa có phản hồi mới.",
                RelatedName = "Không đăng nhập được sau khi đổi mật khẩu",
                CreatedDate = DateTime.Now.AddDays(-1)
            });
            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.Chat,
                Title = "Tin nhắn mới từ Team ERP nội bộ",
                Content = "Nguyễn Văn A: Em phụ trách viết test case cho phần đó.",
                CreatedDate = DateTime.Now.AddDays(-1),
                IsRead = true
            });
            Notifications.Add(new NotificationItemModel
            {
                Type = NotificationType.System,
                Title = "Bảo trì hệ thống định kỳ",
                Content = "Hệ thống sẽ bảo trì từ 23h00 đến 1h00 ngày mai, một số chức năng có thể gián đoạn.",
                CreatedDate = DateTime.Now.AddDays(-2),
                IsRead = true
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Notifications.AsEnumerable();

            if (TypeFilter.HasValue)
                query = query.Where(n => n.Type == TypeFilter.Value);

            if (UnreadOnly)
                query = query.Where(n => !n.IsRead);

            FilteredNotifications.Clear();
            foreach (var n in query.OrderByDescending(n => n.CreatedDate))
                FilteredNotifications.Add(n);

            RecentNotifications.Clear();
            foreach (var n in Notifications.OrderByDescending(n => n.CreatedDate).Take(5))
                RecentNotifications.Add(n);
        }

        private void MarkAsRead(NotificationItemModel item)
        {
            if (item == null || item.IsRead) return;

            // TODO: gọi service PATCH /me/notifications/{id}/read
            item.IsRead = true;
            RaiseCounters();
        }

        private void MarkAllAsRead()
        {
            // TODO: gọi service PATCH /me/notifications/read-all
            foreach (var n in Notifications.Where(n => !n.IsRead))
                n.IsRead = true;

            RaiseCounters();
        }

        private void Delete(NotificationItemModel item)
        {
            if (item == null) return;

            // TODO: gọi service DELETE /me/notifications/{id}
            Notifications.Remove(item);
            ApplyFilter();
            RaiseCounters();
        }

        private void DeleteAll()
        {
            // TODO: gọi service DELETE /me/notifications
            Notifications.Clear();
            ApplyFilter();
            RaiseCounters();
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(UnreadCount));
        }
    }
}