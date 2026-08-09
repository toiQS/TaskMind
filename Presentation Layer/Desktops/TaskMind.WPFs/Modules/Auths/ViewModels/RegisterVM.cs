using System.Text.RegularExpressions;
using System.Windows.Input;
using MediatR;
using TaskMind.WPFs.Utilities;
using AuthRegisterCommand = TaskMind.Applications.Auths.Features.RegisterCommand;
using RegisterResultDto = TaskMind.Applications.Auths.Dtos.RegisterResultDto;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class RegisterVM : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly Action<RegisterResultDto> _onRegisterSuccess;

        private string _fullName;
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _confirmPassword;
        public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }

        private bool _agreeTerms;
        public bool AgreeTerms { get => _agreeTerms; set { _agreeTerms = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand RegisterCommand { get; }

        public RegisterVM() : this(null, null) { }

        public RegisterVM(IMediator mediator, Action<RegisterResultDto> onRegisterSuccess = null)
        {
            _mediator = MediatorResolver.Resolve(mediator);
            _onRegisterSuccess = onRegisterSuccess;

            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName)) { ErrorMessage = "Vui lòng nhập họ và tên."; return; }
            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { ErrorMessage = "Email không hợp lệ."; return; }
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            { ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự."; return; }
            if (Password != ConfirmPassword) { ErrorMessage = "Mật khẩu xác nhận không khớp."; return; }
            if (!AgreeTerms) { ErrorMessage = "Bạn cần đồng ý với điều khoản sử dụng."; return; }

            IsBusy = true;
            try
            {
                var result = await _mediator.Send(new AuthRegisterCommand
                {
                    FullName = FullName.Trim(),
                    Email = Email.Trim(),
                    Password = Password,
                    ConfirmPassword = ConfirmPassword
                });

                _onRegisterSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}