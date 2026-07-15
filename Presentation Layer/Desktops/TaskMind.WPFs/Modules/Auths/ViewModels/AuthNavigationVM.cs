using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Input;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class AuthNavigationVM : ViewModelBase
    {
        private object _authCurrentView;
        public object AuthCurrentView
        {
            get => _authCurrentView;
            set { _authCurrentView = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand {  get; set; }
        public ICommand ForgotPasswordCommand { get; set; }

        public void Login(object obj) => AuthCurrentView = new LoginVM();
        public void Register(object obj) => AuthCurrentView = new RegisterVM();
        public void ForgotPassword(object obj) => AuthCurrentView = new ForgotPasswordVM();
        

        public AuthNavigationVM()
        {
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);

            AuthCurrentView = new LoginVM();
        }
    }
}
