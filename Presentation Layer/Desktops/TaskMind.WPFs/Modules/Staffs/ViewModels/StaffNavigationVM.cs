using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
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
        public ICommand ProfileCommand { get; set; }
        public ICommand ProjectCommand { get; set; }
        public ICommand TodoCommand {  get; set; }

        private void Profile(object obj) => StaffCurrentView = new ProfileVM();
        private void Project(object obj) => StaffCurrentView = new ProjectVM();
        private void Todo(object obj) => StaffCurrentView = new TodoVM();

        public StaffNavigationVM()
        {
            ProfileCommand = new RelayCommand(Profile);
            ProjectCommand = new RelayCommand(Project);
            TodoCommand = new RelayCommand(Todo);

            StaffCurrentView = new TodoVM();
        }
    }
}
