using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class CompanyNavigationVM : ViewModelBase
    {
        private object _companyCurrentView;
        public object CompanyCurrentView
        {
            get => _companyCurrentView;
            set { _companyCurrentView = value; OnPropertyChanged(); }
        }

        public ICommand DashbroadCommand { get; set; }

        private void Dashbroad(object obj) => CompanyCurrentView = new DashbroadVM();

        public CompanyNavigationVM()
        {
            DashbroadCommand = new RelayCommand(Dashbroad);

            CompanyCurrentView = new DashbroadVM();
        }
    }
}
