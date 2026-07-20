using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class StaffNavigationVM : ViewModelBase
    {
        private object _staffCurrentView;
        public object StaffCurrentView
        {
            get => _staffCurrentView;
            set { _staffCurrentView = value; OnPropertyChanged(); }
        }

        public StaffNavigationVM()
        {

        }
    }
}
