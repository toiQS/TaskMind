using System.Windows.Input;
using MediatR;
using TaskMind.Applications.Admins.Features.Skills;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DetailSkillVM : ViewModelBase
    {
        private readonly Action _onBack;
        private readonly IMediator _mediator;

        public string SkillId { get; }

        private DetailSkillModel _detail = new DetailSkillModel();
        public DetailSkillModel Detail
        {
            get => _detail;
            set { _detail = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsPending)); }
        }

        public bool IsPending => Detail?.Skill != null && !Detail.Skill.IsApproved;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand DeleteCommand { get; }

        public DetailSkillVM(string skillId, Action onBack, IMediator mediator)
        {
            SkillId = skillId;
            _onBack = onBack;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ApproveCommand = new RelayCommand(async _ => await ApproveAsync());
            RejectCommand = new RelayCommand(async _ => await RejectAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync());

            _ = LoadDataAsync();
        }

        private async Task ApproveAsync()
        {
            if (Detail?.Skill == null) return;
            var dto = await _mediator.Send(new ApproveSkillCommand { SkillId = Guid.Parse(SkillId) });
            Detail.Skill.IsApproved = dto.IsApproved;
            OnPropertyChanged(nameof(Detail));
            OnPropertyChanged(nameof(IsPending));
        }

        private async Task RejectAsync()
        {
            await _mediator.Send(new RejectSkillCommand { SkillId = Guid.Parse(SkillId) });
            _onBack?.Invoke();
        }

        private async Task DeleteAsync()
        {
            await _mediator.Send(new DeleteSkillCommand { SkillId = Guid.Parse(SkillId) });
            _onBack?.Invoke();
        }

        /// <summary>
        /// UsageCount, RelatedSkills đến từ GetSkillDetailQuery thật.
        /// TotalProjectsRequiring/TotalEndorsements/TopUsers/UsageBySource/GrowthChart/ApprovalHistory:
        /// TODO — chưa có Query tương ứng ở Application.Admins.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            var dto = await _mediator.Send(new GetSkillDetailQuery { SkillId = Guid.Parse(SkillId) });

            var model = new DetailSkillModel { Skill = SkillUiMapper.ToUi(dto) };
            SkillUiMapper.ApplyDetail(model, dto);

            Detail = model;
            IsBusy = false;
        }
    }
}