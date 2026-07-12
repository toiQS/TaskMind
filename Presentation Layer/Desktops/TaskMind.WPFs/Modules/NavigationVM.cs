using System.Windows.Input;
using TaskMind.WPFs.Modules.Auths.ViewModels;
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


        private void Auth(object obj) => CurrentView = new AuthNavigationVM();


        public NavigationVM()
        {
            AuthCommand = new RelayCommand(Auth);

            CurrentView = new AuthNavigationVM();
        }
    }
}
