using System.Windows.Input;
using MediatR;
using TaskMind.WPFs.Utilities;
using AuthLoginCommand = TaskMind.Applications.Auths.Features.LoginCommand;
using LoginResultDto = TaskMind.Applications.Auths.Dtos.LoginResultDto;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class LoginVM : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly Action<LoginResultDto> _onLoginSuccess;

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private bool _rememberMe;
        public bool RememberMe { get => _rememberMe; set { _rememberMe = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }

        public LoginVM() : this(null, null) { }

        public LoginVM(IMediator mediator, Action<LoginResultDto> onLoginSuccess = null)
        {
            _mediator = MediatorResolver.Resolve(mediator);
            _onLoginSuccess = onLoginSuccess;

            LoginCommand = new RelayCommand(async _ => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return;
            }

            IsBusy = true;
            try
            {
                var result = await _mediator.Send(new AuthLoginCommand
                {
                    Email = Email.Trim(),
                    Password = Password,
                    RememberMe = RememberMe
                });

                // TODO: lưu AccessToken/RefreshToken vào nơi lưu phiên đăng nhập (Session/SecureStorage),
                // rồi điều hướng sang trang phù hợp theo result.Role.
                _onLoginSuccess?.Invoke(result);
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