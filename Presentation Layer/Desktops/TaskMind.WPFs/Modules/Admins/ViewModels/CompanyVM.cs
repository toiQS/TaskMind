using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using TaskMind.Applications.Admins.Features.Companies;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class CompanyVM : ViewModelBase
    {
        private readonly Action<object> _navigate;
        private readonly IMediator _mediator;

        public ObservableCollection<CompanyModel> Companies { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

        /// <summary>"All" | "Pending" | "Active" | "Suspended" | "Rejected"</summary>
        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

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

        public CompanyVM() : this(null, null) { }
        public CompanyVM(Action<object> navigate) : this(navigate, null) { }

        public CompanyVM(Action<object> navigate, IMediator mediator)
        {
            _navigate = navigate;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            ApproveCommand = new RelayCommand(async o => await ApproveAsync(o));
            RejectCommand = new RelayCommand(async o => await RejectAsync(o));
            ToggleSuspendCommand = new RelayCommand(async o => await ToggleSuspendAsync(o));
            OpenAddCompanyCommand = new RelayCommand(_ => OpenAddPanel());
            ViewDetailCommand = new RelayCommand(ViewDetail);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_mediator == null || IsBusy) return;
            IsBusy = true;

            var dtos = await _mediator.Send(new GetCompaniesQuery
            {
                SearchText = SearchText,
                StatusFilter = StatusFilter
            });

            Companies.Clear();
            foreach (var dto in dtos)
                Companies.Add(CompanyUiMapper.ToUi(dto));

            IsBusy = false;
        }

        private async Task ApproveAsync(object obj)
        {
            if (obj is not CompanyModel company) return;
            var dto = await _mediator.Send(new ApproveCompanyCommand { CompanyId = Guid.Parse(company.Id) });
            company.Status = Enum.Parse<CompanyStatus>(dto.Status);
            Touch(company);
        }

        private async Task RejectAsync(object obj)
        {
            if (obj is not CompanyModel company) return;
            var dto = await _mediator.Send(new RejectCompanyCommand { CompanyId = Guid.Parse(company.Id) });
            company.Status = Enum.Parse<CompanyStatus>(dto.Status);
            Touch(company);
        }

        private async Task ToggleSuspendAsync(object obj)
        {
            if (obj is not CompanyModel company) return;
            var dto = await _mediator.Send(new ToggleSuspendCompanyCommand { CompanyId = Guid.Parse(company.Id) });
            company.Status = Enum.Parse<CompanyStatus>(dto.Status);
            Touch(company);
        }

        private void OpenAddPanel()
        {
            AddCompanyVM = new AddCompanyVM(OnCompanyCreated, CloseAddPanel, _mediator);
            IsAddPanelOpen = true;
        }

        private void CloseAddPanel()
        {
            IsAddPanelOpen = false;
            AddCompanyVM = null;
        }

        private void OnCompanyCreated(CompanyModel newCompany)
        {
            Companies.Insert(0, newCompany);
            CloseAddPanel();
        }

        private void ViewDetail(object obj)
        {
            if (obj is CompanyModel company && _navigate != null)
            {
                var detailVM = new DetailCompanyVM(company.Id, () => _navigate(this), _mediator);
                _navigate(detailVM);
            }
        }

        /// <summary>CompanyModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item để UI cập nhật.</summary>
        private void Touch(CompanyModel changed)
        {
            int index = Companies.IndexOf(changed);
            if (index >= 0)
            {
                Companies.RemoveAt(index);
                Companies.Insert(index, changed);
            }
        }
    }
}