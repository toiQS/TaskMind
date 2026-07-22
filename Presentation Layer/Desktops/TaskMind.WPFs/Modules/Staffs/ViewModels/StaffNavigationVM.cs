using System.Windows;
using System.Windows.Input;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class StaffNavigationVM : ViewModelBase
    {
        private object _staffCurrentView;
        public object StaffCurrentView
        {
            get => _staffCurrentView;
            set { _staffCurrentView = value; OnPropertyChanged(); }
        }

        public NotificationVM NotificationsVM { get; } = new();

        private string _activeMenu = "Profile";
        public string ActiveMenu { get => _activeMenu; set { _activeMenu = value; OnPropertyChanged(); } }

        public ICommand ProfileCommand { get; }
        public ICommand ProjectCommand { get; }
        public ICommand TodoCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand SupportCommand { get; }
        public ICommand SourceCommand { get; }
        public ICommand OpenNotificationsCommand { get; }

        /// <summary>Thoát khỏi hệ thống — có xác nhận trước khi đóng ứng dụng, tránh mất dữ liệu
        /// đang thao tác dở (VD: đang soạn thảo mã nguồn chưa commit ở module SourceView).</summary>
        public ICommand ExitCommand { get; }

        private void Profile(object obj) { StaffCurrentView = new ProfileVM(); ActiveMenu = "Profile"; }
        private void Project(object obj) { StaffCurrentView = new ProjectVM(); ActiveMenu = "Project"; }
        private void Todo(object obj) { StaffCurrentView = new TodoVM(); ActiveMenu = "Todo"; }
        private void Chat(object obj) { StaffCurrentView = new ChatVM(); ActiveMenu = "Chat"; }
        private void Support(object obj) { StaffCurrentView = new SupportVM(); ActiveMenu = "Support"; }
        private void Source(object obj) { StaffCurrentView = new SourceVM(); ActiveMenu = "Source"; }

        private void OpenNotifications(object obj)
        {
            NotificationsVM.IsPanelOpen = false;
            StaffCurrentView = NotificationsVM;
            ActiveMenu = "Notifications";
        }

        private void Exit(object obj)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn thoát khỏi hệ thống? Các thay đổi chưa lưu (nếu có) có thể bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        public StaffNavigationVM()
        {
            ProfileCommand = new RelayCommand(Profile);
            ProjectCommand = new RelayCommand(Project);
            TodoCommand = new RelayCommand(Todo);
            ChatCommand = new RelayCommand(Chat);
            SupportCommand = new RelayCommand(Support);
            SourceCommand = new RelayCommand(Source);
            OpenNotificationsCommand = new RelayCommand(OpenNotifications);
            ExitCommand = new RelayCommand(Exit);

            StaffCurrentView = new ProfileVM();
        }
    }
}