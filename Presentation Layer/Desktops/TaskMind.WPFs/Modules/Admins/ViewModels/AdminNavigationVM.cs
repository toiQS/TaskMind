using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Auths.ViewModels;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class AdminNavigationVM : ViewModelBase
    {
        private object _adminCurrentView;
        public object AdminCurrentView
        {
            get => _adminCurrentView;
            set { _adminCurrentView = value; OnPropertyChanged(); }
        }

        // Key của menu đang được chọn, dùng để highlight item tương ứng trên sidebar
        private string _activeKey;
        public string ActiveKey
        {
            get => _activeKey;
            set { _activeKey = value; OnPropertyChanged(); }
        }

        public ICommand DashbroadCommand { get; set; }
        public ICommand UserCommand { get; set; }
        public ICommand ProfileCommand { get; set; }
        public ICommand CompanyCommand { get; set; }
        public ICommand SchoolCommand { get; set; }
        public ICommand ProfitCommand { get; set; }
        public ICommand SkillCommand { get; set; }
        public ICommand ReportCommand { get; set; }
        public ICommand ChatCommand { get; set; }
        public ICommand HandlerCommand { get; set; }

        // Lệnh cho khu vực dưới cùng của sidebar
        public ICommand LogoutCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        private void Dashbroad(object obj) { AdminCurrentView = new DashbroadVM(); ActiveKey = "Dashbroad"; }
        private void User(object obj) { AdminCurrentView = new UserVM(NavigateTo); ActiveKey = "User"; }
        private void Profile(object obj) { AdminCurrentView = new ProfileVM(); ActiveKey = "Profile"; }
        private void Skill(object obj) { AdminCurrentView = new SkillVM(NavigateTo); ActiveKey = "Skill"; }
        private void Company(object obj) { AdminCurrentView = new CompanyVM(NavigateTo); ActiveKey = "Company"; }
        private void School(object obj) { AdminCurrentView = new SchoolVM(NavigateTo); ActiveKey = "School"; }
        private void Profit(object obj) { AdminCurrentView = new ProfitVM(); ActiveKey = "Profit"; }
        private void Report(object obj) { AdminCurrentView = new ReportVM(); ActiveKey = "Report"; }
        private void Handler(object obj) { AdminCurrentView = new HandlerVM(); ActiveKey = "Handler"; }
        private void Chat(object obj) { AdminCurrentView = new ChatVM(); ActiveKey = "Chat"; }

        /// <summary>
        /// Cho phép các ViewModel con (vd. CompanyVM, SkillVM) điều hướng thay thế toàn bộ nội dung trang,
        /// dùng khi cần hiển thị 1 view độc lập (không phải overlay) như DetailCompanyView/DetailSkillView.
        /// ActiveKey không đổi khi gọi hàm này, để sidebar vẫn highlight đúng mục cha (vd. "Skill").
        /// </summary>
        private void NavigateTo(object viewModel)
        {
            AdminCurrentView = viewModel;
        }

        private void Logout(object obj)
        {
            // TODO: gọi service đăng xuất (clear token/session) rồi điều hướng về màn hình Login
            var result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Ví dụ: NavigationService.Instance.NavigateTo(new LoginVM());
            }
        }

        private void Exit(object obj)
        {
            var result = MessageBox.Show("Bạn có chắc muốn thoát ứng dụng?", "Thoát",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        public AdminNavigationVM()
        {
            DashbroadCommand = new RelayCommand(Dashbroad);
            UserCommand = new RelayCommand(User);
            ProfileCommand = new RelayCommand(Profile);
            CompanyCommand = new RelayCommand(Company);
            SchoolCommand = new RelayCommand(School);
            ProfitCommand = new RelayCommand(Profit);
            SkillCommand = new RelayCommand(Skill);
            ReportCommand = new RelayCommand(Report);
            HandlerCommand = new RelayCommand(Handler);
            ChatCommand = new RelayCommand(Chat);

            LogoutCommand = new RelayCommand(Logout);
            ExitCommand = new RelayCommand(Exit);

            AdminCurrentView = new DashbroadVM();
            ActiveKey = "Dashbroad";
        }
    }
}