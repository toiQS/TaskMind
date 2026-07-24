using System.Windows.Input;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    /// <summary>Dùng để highlight mục menu đang chọn trên sidebar (RadioButton.IsChecked).</summary>
    public enum CompanyMenuKey
    {
        Dashboard,
        Project,
        Recruitment,
        Candidate,
        Staff,
        Find,
        Store,
        Chat,
        Support
    }

    public class CompanyNavigationVM : ViewModelBase
    {
        private object _companyCurrentView;
        public object CompanyCurrentView
        {
            get => _companyCurrentView;
            set { _companyCurrentView = value; OnPropertyChanged(); }
        }

        private CompanyMenuKey _activeMenu = CompanyMenuKey.Dashboard;
        public CompanyMenuKey ActiveMenu
        {
            get => _activeMenu;
            set { _activeMenu = value; OnPropertyChanged(); }
        }

        public ICommand DashbroadCommand { get; set; }
        public ICommand ProjectCommand { get; set; }
        public ICommand SupportCommand { get; set; }
        public ICommand StoreCommand { get; set; }
        public ICommand RecruitmentCommand { get; set; }
        public ICommand ChatCommand { get; set; }
        public ICommand CandidateCommand { get; set; }
        public ICommand FindCommand { get; set; }
        public ICommand StaffCommand { get; set; }
        public ICommand InformationCommand { get; set; }
        public ICommand NotificationCommand { get; set; }

        private void Dashbroad(object obj) { CompanyCurrentView = new DashbroadVM(); ActiveMenu = CompanyMenuKey.Dashboard; }
        private void Project(object obj) { CompanyCurrentView = new ProjectVM(); ActiveMenu = CompanyMenuKey.Project; }
        private void Support(object obj) { CompanyCurrentView = new SupportVM(); ActiveMenu = CompanyMenuKey.Support; }
        private void Store(object obj) { CompanyCurrentView = new StoreVM(); ActiveMenu = CompanyMenuKey.Store; }
        private void Recruitment(object obj) { CompanyCurrentView = new RecruitmentVM(); ActiveMenu = CompanyMenuKey.Recruitment; }
        private void Chat(object obj) { CompanyCurrentView = new ChatVM(); ActiveMenu = CompanyMenuKey.Chat; }
        private void Candidate(object obj) { CompanyCurrentView = new CandidateVM(); ActiveMenu = CompanyMenuKey.Candidate; }
        private void Find(object obj) { CompanyCurrentView = new FindVM(); ActiveMenu = CompanyMenuKey.Find; }
        private void Staff(object obj) { CompanyCurrentView = new StaffVM(); ActiveMenu = CompanyMenuKey.Staff; }

        // Information/Notification không thuộc menu chính (mở từ nút góc trên phải) nên không đổi ActiveMenu
        private void Information(object obj) => CompanyCurrentView = new InformationVM();
        private void Notification(object obj) => CompanyCurrentView = new NotificationVM();

        public CompanyNavigationVM()
        {
            DashbroadCommand = new RelayCommand(Dashbroad);
            ProjectCommand = new RelayCommand(Project);
            SupportCommand = new RelayCommand(Support);
            StoreCommand = new RelayCommand(Store);
            RecruitmentCommand = new RelayCommand(Recruitment);
            ChatCommand = new RelayCommand(Chat);
            CandidateCommand = new RelayCommand(Candidate);
            FindCommand = new RelayCommand(Find);
            StaffCommand = new RelayCommand(Staff);
            InformationCommand = new RelayCommand(Information);
            NotificationCommand = new RelayCommand(Notification);

            CompanyCurrentView = new DashbroadVM();
            ActiveMenu = CompanyMenuKey.Dashboard;
        }
    }
}