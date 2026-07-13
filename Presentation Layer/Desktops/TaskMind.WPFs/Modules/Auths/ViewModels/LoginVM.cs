using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class LoginVM : ViewModelBase
    {
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

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }

        public LoginVM()
        {
            LoginCommand = new RelayCommand(Login);
        }

        private void Login(object obj)
        {
            // TODO: gọi service xác thực
        }
    }
}
