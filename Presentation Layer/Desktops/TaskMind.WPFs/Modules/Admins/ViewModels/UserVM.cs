using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using TaskMind.Applications.Admins.Features.Users;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class UserVM : ViewModelBase
    {
        private readonly Action<object> _navigate;
        private readonly IMediator _mediator;

        public ObservableCollection<UserModel> Users { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

        /// <summary>"All" | "Active" | "Locked" | "Banned"</summary>
        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

        /// <summary>
        /// GetUsersQuery (Application layer) hiện chưa hỗ trợ lọc theo UserType/Role.
        /// Giữ TypeFilter ở client-side bằng cách lọc lại collection sau khi tải, để không phá UI hiện có.
        /// TODO: bổ sung RoleFilter vào GetUsersQuery nếu muốn lọc phía server.
        /// </summary>
        private string _typeFilter = "All";
        public string TypeFilter
        {
            get => _typeFilter;
            set { _typeFilter = value; OnPropertyChanged(); _ = LoadDataAsync(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterStatusCommand { get; }
        public ICommand FilterTypeCommand { get; }
        public ICommand ToggleLockCommand { get; }
        public ICommand ToggleBanCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public UserVM() : this(null, null) { }
        public UserVM(Action<object> navigate) : this(navigate, null) { }

        public UserVM(Action<object> navigate, IMediator mediator)
        {
            _navigate = navigate;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterStatusCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            FilterTypeCommand = new RelayCommand(f => TypeFilter = f as string ?? "All");
            ToggleLockCommand = new RelayCommand(async o => await ToggleLockAsync(o));
            ToggleBanCommand = new RelayCommand(async o => await ToggleBanAsync(o));
            ViewDetailCommand = new RelayCommand(ViewDetail);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_mediator == null || IsBusy) return;
            IsBusy = true;

            var dtos = await _mediator.Send(new GetUsersQuery
            {
                SearchText = SearchText,
                StatusFilter = StatusFilter
            });

            var mapped = dtos.Select(UserUiMapper.ToUi);
            if (TypeFilter != "All")
                mapped = mapped.Where(u => string.Equals(u.Type.ToString(), TypeFilter, StringComparison.OrdinalIgnoreCase));

            Users.Clear();
            foreach (var u in mapped)
                Users.Add(u);

            IsBusy = false;
        }

        private async Task ToggleLockAsync(object obj)
        {
            if (obj is not UserModel user || user.Status == UserAccountStatus.Banned) return;
            var dto = await _mediator.Send(new ToggleLockUserCommand { UserId = Guid.Parse(user.Id) });
            user.Status = MapStatus(dto.Status);
            Touch(user);
        }

        private async Task ToggleBanAsync(object obj)
        {
            if (obj is not UserModel user) return;
            var dto = await _mediator.Send(new ToggleBanUserCommand { UserId = Guid.Parse(user.Id) });
            user.Status = MapStatus(dto.Status);
            Touch(user);
        }

        private static UserAccountStatus MapStatus(string status) => status switch
        {
            "Paused" => UserAccountStatus.Locked,
            "Blocked" => UserAccountStatus.Banned,
            _ => UserAccountStatus.Active
        };

        private void ViewDetail(object obj)
        {
            if (obj is UserModel user && _navigate != null)
            {
                var detailVM = new DetailUserVM(user.Id, () => _navigate(this), _mediator);
                _navigate(detailVM);
            }
        }

        private void Touch(UserModel changed)
        {
            int index = Users.IndexOf(changed);
            if (index >= 0)
            {
                Users.RemoveAt(index);
                Users.Insert(index, changed);
            }
        }
    }
}