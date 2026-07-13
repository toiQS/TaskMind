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
        

        public AuthNavigationVM()
        {
            LoginCommand = new RelayCommand(Login);

            AuthCurrentView = new LoginVM();
        }
    }
}
