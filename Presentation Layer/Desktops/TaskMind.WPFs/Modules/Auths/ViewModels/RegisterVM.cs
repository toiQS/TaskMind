using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Auths.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class RegisterVM : ViewModelBase
    {
        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        private bool _agreeTerms;
        public bool AgreeTerms
        {
            get => _agreeTerms;
            set { _agreeTerms = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }

        public RegisterVM()
        {
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Vui lòng nhập họ và tên.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorMessage = "Email không hợp lệ.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return;
            }

            if (!AgreeTerms)
            {
                ErrorMessage = "Bạn cần đồng ý với điều khoản sử dụng.";
                return;
            }

            IsBusy = true;

            var model = new RegisterModel
            {
                FullName = FullName.Trim(),
                Email = Email.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                UserRole = UserRole.User,
                AgreeTerms = AgreeTerms
            };

            // TODO: gọi service POST /auth/register (model), xử lý email trùng, gửi email xác thực...
            await Task.Delay(500);

            IsBusy = false;

            // TODO: điều hướng sang màn xác thực email hoặc quay lại LoginVM sau khi đăng ký thành công
        }
    }
}