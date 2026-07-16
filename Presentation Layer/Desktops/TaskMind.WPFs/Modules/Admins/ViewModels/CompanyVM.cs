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
        /// <summary>
        /// Callback điều hướng do AdminNavigationVM truyền vào, dùng để thay thế toàn bộ
        /// AdminCurrentView (vd. chuyển sang DetailCompanyVM) thay vì hiển thị overlay.
        /// Có thể null khi CompanyVM được tạo ở design-time.
        /// </summary>
        private readonly Action<object> _navigate;

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

        // ----- Panel "Thêm công ty mới" (vẫn hiển thị dạng overlay/modal) -----
        private bool _isAddPanelOpen;
        public bool IsAddPanelOpen
        {
            get => _isAddPanelOpen;
            set { _isAddPanelOpen = value; OnPropertyChanged(); }
        }

        private AddCompanyVM _addCompanyVM;
        public AddCompanyVM AddCompanyVM
        {
            get => _addCompanyVM;
            private set { _addCompanyVM = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand ToggleSuspendCommand { get; }
        public ICommand OpenAddCompanyCommand { get; }
        public ICommand ViewDetailCommand { get; }

        /// <summary>Constructor mặc định (dùng khi thiết kế XAML / không cần điều hướng).</summary>
        public CompanyVM() : this(null) { }

        /// <summary>
        /// navigate: callback do AdminNavigationVM cung cấp để thay thế AdminCurrentView,
        /// dùng khi mở DetailCompanyView như một trang độc lập.
        /// </summary>
        public CompanyVM(Action<object> navigate)
        {
            _navigate = navigate;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            ApproveCommand = new RelayCommand(Approve);
            RejectCommand = new RelayCommand(Reject);
            ToggleSuspendCommand = new RelayCommand(ToggleSuspend);
            OpenAddCompanyCommand = new RelayCommand(_ => OpenAddPanel());
            ViewDetailCommand = new RelayCommand(ViewDetail);

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

        /// <summary>Mở panel thêm công ty, luôn tạo mới AddCompanyVM để form trống mỗi lần mở.</summary>
        private void OpenAddPanel()
        {
            AddCompanyVM = new AddCompanyVM(OnCompanyCreated, CloseAddPanel);
            IsAddPanelOpen = true;
        }

        private void CloseAddPanel()
        {
            IsAddPanelOpen = false;
            AddCompanyVM = null;
        }

        /// <summary>Callback khi AddCompanyVM tạo công ty thành công: thêm vào danh sách và đóng panel.</summary>
        private void OnCompanyCreated(CompanyModel newCompany)
        {
            // TODO: sau khi service tạo công ty thành công (POST /companies), có thể gọi lại
            // LoadDataAsync() để đồng bộ dữ liệu thay vì chỉ thêm vào collection tại chỗ.
            Companies.Insert(0, newCompany);
            CloseAddPanel();
        }

        /// <summary>
        /// Điều hướng sang DetailCompanyVM như một trang độc lập (thay thế toàn bộ nội dung),
        /// thay vì hiển thị overlay. Khi bấm "Quay lại" ở DetailCompanyView, callback onBack
        /// sẽ điều hướng ngược lại về chính CompanyVM hiện tại (giữ nguyên filter/search).
        /// </summary>
        private void ViewDetail(object obj)
        {
            if (obj is CompanyModel company && _navigate != null)
            {
                var detailVM = new DetailCompanyVM(company.Id, () => _navigate(this));
                _navigate(detailVM);
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