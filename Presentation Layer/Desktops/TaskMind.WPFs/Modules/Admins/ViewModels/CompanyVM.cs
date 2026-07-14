using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class CompanyVM : ViewModelBase
    {
        public ObservableCollection<CompanyModel> Companies { get; } = new ObservableCollection<CompanyModel>();

        private ICollectionView _companiesView;
        public ICollectionView CompaniesView
        {
            get => _companiesView;
            private set { _companiesView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); CompaniesView?.Refresh(); }
        }

        /// <summary>"All" | "Pending" | "Active" | "Suspended" | "Rejected"</summary>
        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); CompaniesView?.Refresh(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand ToggleSuspendCommand { get; }

        public CompanyVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            ApproveCommand = new RelayCommand(Approve);
            RejectCommand = new RelayCommand(Reject);
            ToggleSuspendCommand = new RelayCommand(ToggleSuspend);

            CompaniesView = CollectionViewSource.GetDefaultView(Companies);
            CompaniesView.Filter = FilterCompanies;

            _ = LoadDataAsync();
        }

        private bool FilterCompanies(object obj)
        {
            if (obj is not CompanyModel company) return false;

            if (StatusFilter != "All" &&
                !string.Equals(company.Status.ToString(), StatusFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                company.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void Approve(object obj)
        {
            if (obj is CompanyModel company)
            {
                company.Status = CompanyStatus.Active;
                // TODO: gọi service cập nhật trạng thái công ty (PUT /companies/{id}/approve)
                Touch(company);
            }
        }

        private void Reject(object obj)
        {
            if (obj is CompanyModel company)
            {
                company.Status = CompanyStatus.Rejected;
                // TODO: gọi service cập nhật trạng thái công ty (PUT /companies/{id}/reject)
                Touch(company);
            }
        }

        private void ToggleSuspend(object obj)
        {
            if (obj is CompanyModel company)
            {
                company.Status = company.Status == CompanyStatus.Suspended
                    ? CompanyStatus.Active
                    : CompanyStatus.Suspended;
                // TODO: gọi service cập nhật trạng thái công ty
                Touch(company);
            }
        }

        /// <summary>
        /// CompanyModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item
        /// để UI + CollectionView filter cập nhật hiển thị.
        /// </summary>
        private void Touch(CompanyModel changed)
        {
            int index = Companies.IndexOf(changed);
            if (index >= 0)
            {
                Companies.RemoveAt(index);
                Companies.Insert(index, changed);
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy danh sách công ty.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Companies.Clear();
            foreach (var c in new[]
            {
                new CompanyModel { Id="C001", Name="FPT Software", TaxCode="0102030405", Field="Phát triển phần mềm", Email="contact@fpt.com", Package="Enterprise", Status=CompanyStatus.Active, JoinedDate=new DateTime(2024,3,10), StaffCount=128, ProjectCount=34 },
                new CompanyModel { Id="C002", Name="NextGen Tech", TaxCode="0304050607", Field="Fintech", Email="hello@nextgen.vn", Package="Pro", Status=CompanyStatus.Pending, JoinedDate=new DateTime(2026,7,1), StaffCount=0, ProjectCount=0 },
                new CompanyModel { Id="C003", Name="CloudBase JSC", TaxCode="0506070809", Field="Cloud & DevOps", Email="info@cloudbase.vn", Package="Starter", Status=CompanyStatus.Pending, JoinedDate=new DateTime(2026,7,10), StaffCount=0, ProjectCount=0 },
                new CompanyModel { Id="C004", Name="Vietsoft Solutions", TaxCode="0607080901", Field="Outsourcing", Email="sales@vietsoft.vn", Package="Pro", Status=CompanyStatus.Suspended, JoinedDate=new DateTime(2023,11,22), StaffCount=45, ProjectCount=12 },
                new CompanyModel { Id="C005", Name="DataWise Corp", TaxCode="0708091011", Field="Data & AI", Email="team@datawise.io", Package="Enterprise", Status=CompanyStatus.Active, JoinedDate=new DateTime(2022,5,4), StaffCount=210, ProjectCount=58 },
                new CompanyModel { Id="C006", Name="ByteForge", TaxCode="0809101112", Field="Game Development", Email="hi@byteforge.dev", Package="Starter", Status=CompanyStatus.Rejected, JoinedDate=new DateTime(2026,6,20), StaffCount=0, ProjectCount=0 },
            })
            {
                Companies.Add(c);
            }

            IsBusy = false;
        }
    }
}