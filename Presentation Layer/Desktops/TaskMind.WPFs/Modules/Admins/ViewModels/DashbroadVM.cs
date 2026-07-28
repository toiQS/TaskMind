using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using MediatR;
using TaskMind.Applications.Admins.Features.Dashboard;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DashbroadVM : ViewModelBase
    {
        private readonly IMediator _mediator;

        private DashbroadStatistic _statistic = new DashbroadStatistic();
        public DashbroadStatistic Statistic
        {
            get => _statistic;
            set { _statistic = value; OnPropertyChanged(); }
        }

        // TODO / Backlog — chưa có Feature tương ứng ở Application.Admins:
        //   TodoList          -> cần nguồn dữ liệu "việc cần xử lý" (gộp Pending Company/School/Skill)
        //   RevenueChart       -> cần GetMonthlyRevenueQuery
        //   Notifications      -> cần Notification bounded context (mục 5.3) triển khai ở Application layer
        // Giữ nguyên các collection này rỗng/mock nhẹ để UI không vỡ, không tự bịa dữ liệu giả lập số liệu tài chính.
        public ObservableCollection<TodoModel> TodoList { get; } = new();
        public ObservableCollection<ChartPoint> RevenueChart { get; } = new();
        public ObservableCollection<NotificationModel> Notifications { get; } = new();

        public int UnreadCount => Notifications.Count(n => !n.IsRead);
        public bool HasUnread => UnreadCount > 0;

        private bool _isNotificationPanelOpen;
        public bool IsNotificationPanelOpen
        {
            get => _isNotificationPanelOpen;
            set { _isNotificationPanelOpen = value; OnPropertyChanged(); }
        }

        private Geometry _chartGeometry = Geometry.Empty;
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ToggleNotificationPanelCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }

        public DashbroadVM() : this(null) { }

        public DashbroadVM(IMediator mediator)
        {
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            ToggleNotificationPanelCommand = new RelayCommand(_ => IsNotificationPanelOpen = !IsNotificationPanelOpen);
            MarkAsReadCommand = new RelayCommand(MarkAsRead);
            MarkAllAsReadCommand = new RelayCommand(_ => MarkAllAsRead());

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_mediator == null || IsBusy) return;
            IsBusy = true;

            var dto = await _mediator.Send(new GetDashboardStatisticsQuery { RecentDays = 30 });

            Statistic = new DashbroadStatistic
            {
                CountAllUsers = dto.CountAllUsers,
                CountNewUsers = dto.CountNewUsers,
                CountAllProject = dto.CountAllProjects,
                CountNewProjects = dto.CountNewProjects,
                CountAllCompanies = dto.CountAllCompanies,
                CountNewCompanies = dto.CountNewCompanies,
                CountAllSchools = dto.CountAllSchools,
                CountNewSchools = dto.CountNewSchools,
                CountAllTeachers = dto.CountAllTeachers,
                CountNewTeacher = dto.CountNewTeachers,
                CountAllStaff = dto.CountAllStaff,
                CountNewStaff = dto.CountNewStaff
            };

            IsBusy = false;
        }

        private void MarkAsRead(object obj)
        {
            if (obj is NotificationModel notification && !notification.IsRead)
            {
                notification.IsRead = true;
                Touch(notification);
                RaiseNotificationSummaryChanged();
            }
        }

        private void MarkAllAsRead()
        {
            foreach (var n in Notifications.Where(x => !x.IsRead).ToList())
            {
                n.IsRead = true;
                Touch(n);
            }
            RaiseNotificationSummaryChanged();
        }

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
    }
}