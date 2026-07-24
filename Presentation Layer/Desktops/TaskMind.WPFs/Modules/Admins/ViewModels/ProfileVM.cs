using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class ProfileVM : ViewModelBase
    {
        private ProfileModel _profile = new ProfileModel();
        public ProfileModel Profile
        {
            get => _profile;
            set { _profile = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // ----- Đổi mật khẩu -----
        private string _currentPassword;
        public string CurrentPassword
        {
            get => _currentPassword;
            set { _currentPassword = value; OnPropertyChanged(); }
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        private string _passwordMessage;
        public string PasswordMessage
        {
            get => _passwordMessage;
            set { _passwordMessage = value; OnPropertyChanged(); }
        }

        public ICommand ToggleEditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        public ProfileVM()
        {
            ToggleEditCommand = new RelayCommand(_ => IsEditing = !IsEditing);
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            ChangePasswordCommand = new RelayCommand(async _ => await ChangePasswordAsync());

            _ = LoadAsync();
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy hồ sơ Admin đang đăng nhập.
        /// </summary>
        private async Task LoadAsync()
        {
            IsBusy = true;
            await Task.Delay(300);

            Profile = new ProfileModel
            {
                FullName = "Nguyễn Văn Admin",
                AvatarUrl = null,
                Role = "Quản trị viên hệ thống",
                Email = "admin@taskmind.vn",
                Phone = "0901 234 567",
                DateOfBirth = new DateTime(1996, 5, 20),
                Bio = "Quản trị và giám sát hoạt động toàn hệ thống TaskMind.",
                GithubUrl = "github.com/taskmind-admin",
                LinkedinUrl = "linkedin.com/in/taskmind-admin",
                WebsiteUrl = "",
                IsProfilePublic = false,
                JoinedDate = new DateTime(2023, 1, 15),
                ManagedCompanies = 54,
                ManagedSchools = 21,
                PendingApprovals = 6
            };

            IsBusy = false;
        }

        /// <summary>
        /// TODO: gọi service PUT /admin/profile để lưu thay đổi.
        /// </summary>
        private async Task SaveAsync()
        {
            IsBusy = true;
            await Task.Delay(300);
            IsBusy = false;
            IsEditing = false;
        }

        /// <summary>
        /// TODO: gọi service đổi mật khẩu thật (xác thực CurrentPassword trước khi đổi).
        /// </summary>
        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
            {
                PasswordMessage = "Vui lòng nhập đầy đủ thông tin.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                PasswordMessage = "Mật khẩu xác nhận không khớp.";
                return;
            }

            IsBusy = true;
            await Task.Delay(400);
            IsBusy = false;

            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            PasswordMessage = "Đổi mật khẩu thành công.";
        }
    }
}