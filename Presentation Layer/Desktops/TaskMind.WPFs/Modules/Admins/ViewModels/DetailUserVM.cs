using System.Windows.Input;
using MediatR;
using TaskMind.Applications.Admins.Features.Users;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DetailUserVM : ViewModelBase
    {
        private readonly Action _onBack;
        private readonly IMediator _mediator;

        public string UserId { get; }

        private DetailUserModel _detail = new DetailUserModel();
        public DetailUserModel Detail
        {
            get => _detail;
            set { _detail = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ToggleLockCommand { get; }
        public ICommand ToggleBanCommand { get; }

        public DetailUserVM(string userId, Action onBack, IMediator mediator)
        {
            UserId = userId;
            _onBack = onBack;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ToggleLockCommand = new RelayCommand(async _ => await ToggleLockAsync());
            ToggleBanCommand = new RelayCommand(async _ => await ToggleBanAsync());

            _ = LoadDataAsync();
        }

        private async Task ToggleLockAsync()
        {
            if (Detail?.User == null || Detail.User.Status == UserAccountStatus.Banned) return;
            var dto = await _mediator.Send(new ToggleLockUserCommand { UserId = Guid.Parse(UserId) });
            Detail.User.Status = MapStatus(dto.Status);
            OnPropertyChanged(nameof(Detail));
        }

        private async Task ToggleBanAsync()
        {
            if (Detail?.User == null) return;
            var dto = await _mediator.Send(new ToggleBanUserCommand { UserId = Guid.Parse(UserId) });
            Detail.User.Status = MapStatus(dto.Status);
            OnPropertyChanged(nameof(Detail));
        }

        private static UserAccountStatus MapStatus(string status) => status switch
        {
            "Paused" => UserAccountStatus.Locked,
            "Blocked" => UserAccountStatus.Banned,
            _ => UserAccountStatus.Active
        };

        /// <summary>
        /// Skills/ProjectHistory/AuditLogs đến từ GetUserDetailQuery thật.
        /// Reviews (mục 5.2) và Reports (báo cáo vi phạm) chưa có Query tương ứng — giữ rỗng, TODO bổ sung.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            var dto = await _mediator.Send(new GetUserDetailQuery { UserId = Guid.Parse(UserId) });

            var model = new DetailUserModel { User = UserUiMapper.ToUi(dto) };
            UserUiMapper.ApplyDetail(model, dto);

            model.TotalReviews = model.Reviews.Count;
            model.AverageRating = model.Reviews.Count > 0 ? Math.Round(model.Reviews.Average(r => r.Rating), 1) : 0;

            Detail = model;
            IsBusy = false;
        }
    }
}