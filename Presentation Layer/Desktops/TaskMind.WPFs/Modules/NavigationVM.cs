using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.ViewModels;
using TaskMind.WPFs.Modules.Auths.ViewModels;
using TaskMind.WPFs.Modules.Companies.ViewModels;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules
{
    public class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand AuthCommand { get; set; }
        public ICommand AdminCommand {  get; set; }
        public ICommand CompanyCommand { get; set; }


        private void Auth(object obj) => CurrentView = new AuthNavigationVM();
        private void Admin(object obj) => CurrentView = new AdminNavigationVM();
        private void Company(object obj) => CurrentView = new CompanyNavigationVM();


        public NavigationVM()
        {
            AuthCommand = new RelayCommand(Auth);
            AdminCommand = new RelayCommand(Admin);
            CompanyCommand = new RelayCommand(Company);
            //CurrentView = new AuthNavigationVM();
            //CurrentView = new AdminNavigationVM();
            CurrentView = new CompanyNavigationVM();
        }
    }
}
