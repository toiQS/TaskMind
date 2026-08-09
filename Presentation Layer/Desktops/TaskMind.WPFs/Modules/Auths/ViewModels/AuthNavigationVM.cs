using System.Windows.Input;
using MediatR;
using TaskMind.Applications.Auths.Dtos;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class AuthNavigationVM : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly Action<LoginResultDto> _onLoginSuccess;

        private object _authCurrentView;
        public object AuthCurrentView
        {
            get => _authCurrentView;
            set { _authCurrentView = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand ForgotPasswordCommand { get; set; }

        public void Login(object obj) => AuthCurrentView = new LoginVM(_mediator, _onLoginSuccess);
        public void Register(object obj) => AuthCurrentView = new RegisterVM(_mediator, _ => Login(null));
        public void ForgotPassword(object obj) => AuthCurrentView = new ForgotPasswordVM(_mediator);

        public AuthNavigationVM() : this(null, null) { }

        public AuthNavigationVM(IMediator mediator, Action<LoginResultDto> onLoginSuccess)
        {
            _mediator = MediatorResolver.Resolve(mediator);
            _onLoginSuccess = onLoginSuccess;

            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);

            AuthCurrentView = new LoginVM(_mediator, _onLoginSuccess);
        }
    }
}