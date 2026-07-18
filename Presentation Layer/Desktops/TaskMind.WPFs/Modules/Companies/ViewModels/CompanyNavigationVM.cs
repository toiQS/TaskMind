using System;
using System.Collections.Generic;
using System.Data;
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
        public ICommand ProjectCommand { get; set; }
        public ICommand SupportCommand { get; set; }
        public ICommand StoreCommand { get; set; }
        public ICommand RecruitmentCommand { get; set; }
        public ICommand ChatCommand { get; set; }
        public ICommand CandidateCommand { get; set; }

        private void Dashbroad(object obj) => CompanyCurrentView = new DashbroadVM();
        private void Project(object obj) => CompanyCurrentView = new ProjectVM();
        private void Support(object obj) => CompanyCurrentView = new SupportVM();
        private void Store(object obj) => CompanyCurrentView = new StoreVM();
        private void Recruitment(object obj) => CompanyCurrentView = new RecruitmentVM();
        private void Chat(object obj) => CompanyCurrentView = new ChatVM();
        private void Candidate(object obj) => CompanyCurrentView = new CandidateVM();

        public CompanyNavigationVM()
        {
            DashbroadCommand = new RelayCommand(Dashbroad);
            ProjectCommand = new RelayCommand(Project);
            SupportCommand = new RelayCommand(Support);
            StoreCommand = new RelayCommand(Store);
            RecruitmentCommand = new RelayCommand(Recruitment);
            ChatCommand = new RelayCommand(Chat);
            CandidateCommand = new RelayCommand(Candidate);
            //CompanyCurrentView = new DashbroadVM();
            CompanyCurrentView = new CandidateVM();
        }
    }
}