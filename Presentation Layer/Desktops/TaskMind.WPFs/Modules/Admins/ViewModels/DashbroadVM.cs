using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DashbroadVM : ViewModelBase
    {
        private DashbroadStatistic _statistic = new DashbroadStatistic();
        public DashbroadStatistic Statistic
        {
            get => _statistic;
            set { _statistic = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TodoModel> TodoList { get; } = new ObservableCollection<TodoModel>();

        public ObservableCollection<ChartPoint> RevenueChart { get; } = new ObservableCollection<ChartPoint>();

        /// <summary>Danh sách thông báo hệ thống, mới nhất lên đầu.</summary>
        public ObservableCollection<NotificationModel> Notifications { get; } = new ObservableCollection<NotificationModel>();

        /// <summary>Số thông báo chưa đọc, dùng để hiện badge trên chuông thông báo.</summary>
        public int UnreadCount => Notifications.Count(n => !n.IsRead);

        /// <summary>true khi có ít nhất 1 thông báo chưa đọc, dùng để ẩn/hiện badge.</summary>
        public bool HasUnread => UnreadCount > 0;

        private bool _isNotificationPanelOpen;
        /// <summary>Đóng/mở dropdown danh sách thông báo khi bấm chuông.</summary>
        public bool IsNotificationPanelOpen
        {
            get => _isNotificationPanelOpen;
            set { _isNotificationPanelOpen = value; OnPropertyChanged(); }
        }

        private Geometry _chartGeometry = Geometry.Empty;
        /// <summary>
        /// Đường line chart đã được dựng sẵn (Path.Data bind trực tiếp vào đây)
        /// </summary>
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        /// <summary>Dùng để hiện ui:ProgressRing khi đang tải/làm mới dữ liệu</summary>
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ToggleNotificationPanelCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }

        public DashbroadVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            ToggleNotificationPanelCommand = new RelayCommand(_ => IsNotificationPanelOpen = !IsNotificationPanelOpen);
            MarkAsReadCommand = new RelayCommand(MarkAsRead);
            MarkAllAsReadCommand = new RelayCommand(_ => MarkAllAsRead());

            _ = LoadDataAsync();
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy dữ liệu Dashboard.
        /// Hiện tại đang seed dữ liệu mẫu để dựng giao diện.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            // TODO: thay Task.Delay bằng await _dashboardService.GetDataAsync()
            await Task.Delay(400);

            Statistic = new DashbroadStatistic
            {
                CountAllUsers = 1240,
                CountNewUsers = 38,
                CountAllProject = 312,
                CountNewProjects = 12,
                CountAllCompanies = 54,
                CountNewCompanies = 3,
                CountAllSchools = 21,
                CountNewSchools = 1,
                CountAllTeachers = 87,
                CountNewTeacher = 4,
                CountAllStaff = 210,
                CountNewStaff = 9
            };

            TodoList.Clear();
            foreach (var todo in new[]
            {
                new TodoModel { Index = "1", Name = "Duyệt công ty mới đăng ký", PriorityLevel = 1 },
                new TodoModel { Index = "2", Name = "Xử lý báo cáo vi phạm", PriorityLevel = 1 },
                new TodoModel { Index = "3", Name = "Duyệt đề xuất kỹ năng mới", PriorityLevel = 2 },
                new TodoModel { Index = "4", Name = "Kiểm tra hoá đơn tháng", PriorityLevel = 3 },
                new TodoModel { Index = "5", Name = "Phản hồi ticket hỗ trợ", PriorityLevel = 2 },
            })
            {
                TodoList.Add(todo);
            }

            RevenueChart.Clear();
            foreach (var point in new[]
            {
                new ChartPoint { Label = "T1", Value = 120 },
                new ChartPoint { Label = "T2", Value = 180 },
                new ChartPoint { Label = "T3", Value = 150 },
                new ChartPoint { Label = "T4", Value = 220 },
                new ChartPoint { Label = "T5", Value = 260 },
                new ChartPoint { Label = "T6", Value = 300 },
            })
            {
                RevenueChart.Add(point);
            }

            ChartGeometry = BuildChartGeometry(RevenueChart, width: 600, height: 160, padding: 12);

            // TODO: thay bằng gọi service/API thực tế lấy thông báo hệ thống của Admin đang đăng nhập.
            Notifications.Clear();
            foreach (var n in new[]
            {
                new NotificationModel { Id = "N001", Title = "Công ty mới chờ duyệt", Message = "CloudBase JSC vừa đăng ký, cần Admin kiểm duyệt.", Type = NotificationType.Approval, CreatedDate = DateTime.Now.AddMinutes(-15), IsRead = false },
                new NotificationModel { Id = "N002", Title = "Cơ sở đào tạo mới chờ duyệt", Message = "DevMaster Institute vừa gửi yêu cầu tham gia hệ thống.", Type = NotificationType.Approval, CreatedDate = DateTime.Now.AddHours(-2), IsRead = false },
                new NotificationModel { Id = "N003", Title = "Cảnh báo vi phạm", Message = "Tài khoản anh.vu@spam.net bị báo cáo spam nhiều lần.", Type = NotificationType.Warning, CreatedDate = DateTime.Now.AddHours(-5), IsRead = false },
                new NotificationModel { Id = "N004", Title = "Đề xuất kỹ năng mới", Message = "FUNiX Academy đề xuất thêm kỹ năng \"Tư duy phản biện\".", Type = NotificationType.System, CreatedDate = DateTime.Now.AddDays(-1), IsRead = true },
                new NotificationModel { Id = "N005", Title = "Đổi mật khẩu thành công", Message = "Mật khẩu quản trị viên đã được cập nhật.", Type = NotificationType.Success, CreatedDate = DateTime.Now.AddDays(-2), IsRead = true },
            })
            {
                Notifications.Add(n);
            }
            RaiseNotificationSummaryChanged();

            IsBusy = false;
        }

        private void MarkAsRead(object obj)
        {
            if (obj is NotificationModel notification && !notification.IsRead)
            {
                notification.IsRead = true;
                // TODO: gọi service PUT /notifications/{id}/read
                Touch(notification);
                RaiseNotificationSummaryChanged();
            }
        }

        private void MarkAllAsRead()
        {
            foreach (var n in Notifications.Where(x => !x.IsRead).ToList())
            {
                n.IsRead = true;
                // TODO: gọi service PUT /notifications/read-all
                Touch(n);
            }
            RaiseNotificationSummaryChanged();
        }

        /// <summary>NotificationModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item để UI + badge cập nhật.</summary>
        private void Touch(NotificationModel changed)
        {
            int index = Notifications.IndexOf(changed);
            if (index >= 0)
            {
                Notifications.RemoveAt(index);
                Notifications.Insert(index, changed);
            }
        }

        private void RaiseNotificationSummaryChanged()
        {
            OnPropertyChanged(nameof(UnreadCount));
            OnPropertyChanged(nameof(HasUnread));
        }

        /// <summary>
        /// Dựng Geometry cho line chart từ danh sách ChartPoint, không cần thư viện chart ngoài.
        /// </summary>
        private Geometry BuildChartGeometry(ObservableCollection<ChartPoint> points, double width, double height, double padding)
        {
            if (points == null || points.Count == 0)
                return Geometry.Empty;

            double max = points.Max(p => p.Value);
            double min = points.Min(p => p.Value);
            if (max == min) max = min + 1;

            double stepX = points.Count > 1 ? (width - padding * 2) / (points.Count - 1) : 0;

            var geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                for (int i = 0; i < points.Count; i++)
                {
                    double x = padding + i * stepX;
                    double normalized = (points[i].Value - min) / (max - min);
                    double y = height - padding - normalized * (height - padding * 2);

                    if (i == 0)
                        ctx.BeginFigure(new Point(x, y), isFilled: false, isClosed: false);
                    else
                        ctx.LineTo(new Point(x, y), isStroked: true, isSmoothJoin: true);
                }
            }
            geometry.Freeze();
            return geometry;
        }
    }
}