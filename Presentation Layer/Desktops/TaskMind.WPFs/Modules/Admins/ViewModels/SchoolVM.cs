using MediatR;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.Applications.Admins.Features.Schools;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class SchoolVM : ViewModelBase
    {
        private readonly Action<object> _navigate;
        private readonly IMediator _mediator;

        public ObservableCollection<SchoolModel> Schools { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

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

        private AddSchoolVM _addSchoolVM;
        public AddSchoolVM AddSchoolVM
        {
            get => _addSchoolVM;
            private set { _addSchoolVM = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand ToggleSuspendCommand { get; }
        public ICommand OpenAddSchoolCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public SchoolVM() : this(null, null) { }
        public SchoolVM(Action<object> navigate) : this(navigate, null) { }

        public SchoolVM(Action<object> navigate, IMediator mediator)
        {
            _navigate = navigate;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            ApproveCommand = new RelayCommand(async o => await ApproveAsync(o));
            RejectCommand = new RelayCommand(async o => await RejectAsync(o));
            ToggleSuspendCommand = new RelayCommand(async o => await ToggleSuspendAsync(o));
            OpenAddSchoolCommand = new RelayCommand(_ => OpenAddPanel());
            ViewDetailCommand = new RelayCommand(ViewDetail);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_mediator == null || IsBusy) return;
            IsBusy = true;

            var dtos = await _mediator.Send(new GetSchoolsQuery
            {
                SearchText = SearchText,
                StatusFilter = StatusFilter
            });

            Schools.Clear();
            foreach (var dto in dtos)
                Schools.Add(SchoolUiMapper.ToUi(dto));

            IsBusy = false;
        }

        private async Task ApproveAsync(object obj)
        {
            if (obj is not SchoolModel school) return;
            var dto = await _mediator.Send(new ApproveSchoolCommand { SchoolId = Guid.Parse(school.Id) });
            school.Status = Enum.Parse<SchoolStatus>(dto.Status);
            Touch(school);
        }

        private async Task RejectAsync(object obj)
        {
            if (obj is not SchoolModel school) return;
            var dto = await _mediator.Send(new RejectSchoolCommand { SchoolId = Guid.Parse(school.Id) });
            school.Status = Enum.Parse<SchoolStatus>(dto.Status);
            Touch(school);
        }

        private async Task ToggleSuspendAsync(object obj)
        {
            if (obj is not SchoolModel school) return;
            var dto = await _mediator.Send(new ToggleSuspendSchoolCommand { SchoolId = Guid.Parse(school.Id) });
            school.Status = Enum.Parse<SchoolStatus>(dto.Status);
            Touch(school);
        }

        private void OpenAddPanel()
        {
            AddSchoolVM = new AddSchoolVM(OnSchoolCreated, CloseAddPanel, _mediator);
            IsAddPanelOpen = true;
        }

        private void CloseAddPanel()
        {
            IsAddPanelOpen = false;
            AddSchoolVM = null;
        }

        private void OnSchoolCreated(SchoolModel newSchool)
        {
            Schools.Insert(0, newSchool);
            CloseAddPanel();
        }

        private void ViewDetail(object obj)
        {
            if (obj is SchoolModel school && _navigate != null)
            {
                var detailVM = new DetailSchoolVM(school.Id, () => _navigate(this), _mediator);
                _navigate(detailVM);
            }
        }

        private void Touch(SchoolModel changed)
        {
            int index = Schools.IndexOf(changed);
            if (index >= 0)
            {
                Schools.RemoveAt(index);
                Schools.Insert(index, changed);
            }
        }
    }
}