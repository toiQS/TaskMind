using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Auths.ViewModels;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class AdminNavigationVM : ViewModelBase
    {
        private object _adminCurrentView;
        public object AdminCurrentView
        {
            get => _adminCurrentView;
            set { _adminCurrentView = value;  OnPropertyChanged(); }
        }


        public ICommand DashbroadCommand { get; set; }
        public ICommand UserCommand { get; set; }
        public ICommand ProfileCommand { get; set; }
        public ICommand CompanyCommand { get; set; }
        public ICommand SchoolCommand { get; set; }
        public ICommand ProfitCommand { get; set; }
        public ICommand SkillCommand { get; set; }
        public ICommand ReportCommand {  get; set; }

        private void Dashbroad(object obj) => AdminCurrentView = new DashbroadVM();
        private void User(object obj) => AdminCurrentView = new UserVM();
        private void Profile(object obj) => AdminCurrentView = new ProfileVM();
        private void Skill(object obj) => AdminCurrentView = new SkillVM();
        private void Company(object obj) => AdminCurrentView = new CompanyVM();
        private void School(object obj) => AdminCurrentView = new SchoolVM();
        private void Profit(object obj) => AdminCurrentView = new ProfitVM();
        private void Report(object obj) => AdminCurrentView = new ReportVM();


        public AdminNavigationVM()
        {
            DashbroadCommand = new RelayCommand(Dashbroad);
            UserCommand = new RelayCommand(User);
            ProfileCommand = new RelayCommand(Profile);
            CompanyCommand = new RelayCommand(Company);
            SchoolCommand = new RelayCommand(School);
            ProfitCommand = new RelayCommand(Profit);
            SkillCommand = new RelayCommand(Skill);
            ReportCommand = new RelayCommand(Report);

            AdminCurrentView = new DashbroadVM();
        }
    }
}
