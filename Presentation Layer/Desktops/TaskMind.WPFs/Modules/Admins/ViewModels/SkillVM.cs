using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using MediatR;
using TaskMind.Applications.Admins.Features.Skills;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class SkillVM : ViewModelBase
    {
        private readonly Action<object> _navigate;
        private readonly IMediator _mediator;

        public ObservableCollection<SkillModel> Skills { get; } = new();

        private ICollectionView _skillsView;
        public ICollectionView SkillsView
        {
            get => _skillsView;
            private set { _skillsView = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SkillModel> PendingSkillsSource { get; } = new();

        private ICollectionView _pendingSkillsView;
        public ICollectionView PendingSkillsView
        {
            get => _pendingSkillsView;
            private set { _pendingSkillsView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

        private string _categoryFilter = "All";
        public string CategoryFilter
        {
            get => _categoryFilter;
            set { _categoryFilter = value; OnPropertyChanged(); _ = LoadDataAsync(); }
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

        private string _newSkillName;
        public string NewSkillName
        {
            get => _newSkillName;
            set { _newSkillName = value; OnPropertyChanged(); }
        }

        public Array CategoryOptions => Enum.GetValues(typeof(SkillCategory));
        public Array LevelOptions => Enum.GetValues(typeof(SkillLevel));

        private SkillCategory _newSkillCategory = SkillCategory.ProgrammingLanguage;
        public SkillCategory NewSkillCategory
        {
            get => _newSkillCategory;
            set { _newSkillCategory = value; OnPropertyChanged(); }
        }

        // SkillDto/AddSkillCommand không có Level -> giữ property để không vỡ XAML nhưng không gửi lên server.
        private SkillLevel _newSkillLevel = SkillLevel.Beginner;
        public SkillLevel NewSkillLevel
        {
            get => _newSkillLevel;
            set { _newSkillLevel = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ToggleAddPanelCommand { get; }
        public ICommand AddSkillCommand { get; }
        public ICommand DeleteSkillCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public SkillVM() : this(null, null) { }
        public SkillVM(Action<object> navigate) : this(navigate, null) { }

        public SkillVM(Action<object> navigate, IMediator mediator)
        {
            _navigate = navigate;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => CategoryFilter = f as string ?? "All");
            ToggleAddPanelCommand = new RelayCommand(_ => IsAddPanelOpen = !IsAddPanelOpen);
            AddSkillCommand = new RelayCommand(async _ => await AddSkillAsync());
            DeleteSkillCommand = new RelayCommand(async o => await DeleteSkillAsync(o));
            ApproveCommand = new RelayCommand(async o => await ApproveAsync(o));
            RejectCommand = new RelayCommand(async o => await RejectAsync(o));
            ViewDetailCommand = new RelayCommand(ViewDetail);

            SkillsView = CollectionViewSource.GetDefaultView(Skills);
            SkillsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SkillModel.Category)));
            SkillsView.SortDescriptions.Add(new SortDescription(nameof(SkillModel.Name), ListSortDirection.Ascending));

            PendingSkillsView = CollectionViewSource.GetDefaultView(PendingSkillsSource);

            _ = LoadDataAsync();
        }

        /// <summary>
        /// GetSkillsQuery hỗ trợ IsApproved -> gọi 2 lần (true cho danh mục chính thức, false cho chờ duyệt)
        /// thay vì lọc client như bản mock cũ.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (_mediator == null || IsBusy) return;
            IsBusy = true;

            var approved = await _mediator.Send(new GetSkillsQuery
            {
                SearchText = SearchText,
                CategoryFilter = CategoryFilter,
                IsApproved = true
            });

            var pending = await _mediator.Send(new GetSkillsQuery { IsApproved = false });

            Skills.Clear();
            foreach (var dto in approved)
                Skills.Add(SkillUiMapper.ToUi(dto));

            PendingSkillsSource.Clear();
            foreach (var dto in pending)
                PendingSkillsSource.Add(SkillUiMapper.ToUi(dto));

            IsBusy = false;
        }

        private async Task AddSkillAsync()
        {
            if (string.IsNullOrWhiteSpace(NewSkillName)) return;

            var dto = await _mediator.Send(new AddSkillCommand
            {
                Name = NewSkillName.Trim(),
                Category = Enum.Parse<TaskMind.Domain.Enums.SkillCategory>(NewSkillCategory.ToString())
            });

            Skills.Add(SkillUiMapper.ToUi(dto));

            NewSkillName = string.Empty;
            IsAddPanelOpen = false;
        }

        private async Task DeleteSkillAsync(object obj)
        {
            if (obj is not SkillModel skill) return;
            await _mediator.Send(new DeleteSkillCommand { SkillId = Guid.Parse(skill.Id) });
            Skills.Remove(skill);
        }

        private async Task ApproveAsync(object obj)
        {
            if (obj is not SkillModel skill) return;
            var dto = await _mediator.Send(new ApproveSkillCommand { SkillId = Guid.Parse(skill.Id) });
            PendingSkillsSource.Remove(skill);
            Skills.Add(SkillUiMapper.ToUi(dto));
        }

        private async Task RejectAsync(object obj)
        {
            if (obj is not SkillModel skill) return;
            await _mediator.Send(new RejectSkillCommand { SkillId = Guid.Parse(skill.Id) });
            PendingSkillsSource.Remove(skill);
        }

        private void ViewDetail(object obj)
        {
            if (obj is SkillModel skill && _navigate != null)
            {
                var detailVM = new DetailSkillVM(skill.Id, () => _navigate(this), _mediator);
                _navigate(detailVM);
            }
        }
    }
}