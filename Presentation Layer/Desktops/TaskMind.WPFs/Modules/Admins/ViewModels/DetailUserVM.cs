using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DetailUserVM : ViewModelBase
    {
        private readonly Action _onBack;

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

        /// <summary>
        /// userId: mã người dùng cần xem chi tiết.
        /// onBack: callback gọi khi bấm "Quay lại", do UserVM cung cấp để điều hướng
        /// ngược lại về chính UserVM hiện tại (giữ nguyên filter/search đang chọn).
        /// </summary>
        public DetailUserVM(string userId, Action onBack)
        {
            UserId = userId;
            _onBack = onBack;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ToggleLockCommand = new RelayCommand(_ => ToggleLock());
            ToggleBanCommand = new RelayCommand(_ => ToggleBan());

            _ = LoadDataAsync();
        }

        private void ToggleLock()
        {
            if (Detail?.User == null || Detail.User.Status == UserAccountStatus.Banned) return;

            Detail.User.Status = Detail.User.Status == UserAccountStatus.Locked
                ? UserAccountStatus.Active
                : UserAccountStatus.Locked;

            // TODO: gọi service PUT /users/{id}/lock hoặc /unlock
            AppendAuditLog(Detail.User.Status == UserAccountStatus.Locked ? "Tạm khoá tài khoản" : "Mở khoá tài khoản");
            OnPropertyChanged(nameof(Detail));
        }

        private void ToggleBan()
        {
            if (Detail?.User == null) return;

            Detail.User.Status = Detail.User.Status == UserAccountStatus.Banned
                ? UserAccountStatus.Active
                : UserAccountStatus.Banned;

            // TODO: gọi service PUT /users/{id}/ban hoặc /unban
            AppendAuditLog(Detail.User.Status == UserAccountStatus.Banned ? "Cấm tài khoản" : "Gỡ cấm tài khoản");
            OnPropertyChanged(nameof(Detail));
        }

        private void AppendAuditLog(string action)
        {
            Detail.AuditLogs.Insert(0, new AuditLogEntryModel
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                EntityId = UserId,
                Action = action,
                Description = "Thao tác được thực hiện từ trang chi tiết người dùng.",
                PerformedBy = "Admin",
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy chi tiết user theo UserId:
        /// thông tin cá nhân, hồ sơ kỹ năng, lịch sử dự án, báo cáo vi phạm, audit log.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            var model = new DetailUserModel
            {
                User = new UserModel
                {
                    Id = UserId,
                    FullName = "Phạm Gia Huy",
                    Email = "huy.pham@dev.io",
                    Type = UserType.OssContributor,
                    Status = UserAccountStatus.Active,
                    JoinedDate = new DateTime(2023, 5, 3),
                    LastActiveDate = new DateTime(2026, 7, 14),
                    SkillCount = 14,
                    ProjectCount = 21
                }
            };

            model.Skills.Add(new UserSkillItem { SkillName = "C#", Category = SkillCategory.ProgrammingLanguage, Level = SkillLevel.Advanced, EndorsementCount = 12 });
            model.Skills.Add(new UserSkillItem { SkillName = "React", Category = SkillCategory.Framework, Level = SkillLevel.Intermediate, EndorsementCount = 6 });
            model.Skills.Add(new UserSkillItem { SkillName = "Docker", Category = SkillCategory.Tool, Level = SkillLevel.Advanced, EndorsementCount = 8 });
            model.Skills.Add(new UserSkillItem { SkillName = "Làm việc nhóm", Category = SkillCategory.SoftSkill, Level = SkillLevel.Expert, EndorsementCount = 15 });

            model.ProjectHistory.Add(new UserProjectHistoryItem { ProjectName = "TaskMind Core Platform", ProjectRole = "Technical leader", ProjectSource = "OpenSource", StartDate = new DateTime(2025, 1, 10), IsOngoing = true });
            model.ProjectHistory.Add(new UserProjectHistoryItem { ProjectName = "E-commerce API cho DataWise Corp", ProjectRole = "Developer", ProjectSource = "Company", StartDate = new DateTime(2024, 6, 1), EndDate = new DateTime(2024, 11, 20), IsOngoing = false });
            model.ProjectHistory.Add(new UserProjectHistoryItem { ProjectName = "Dự án thực hành React Native", ProjectRole = "Developer", ProjectSource = "School", StartDate = new DateTime(2023, 9, 1), EndDate = new DateTime(2023, 12, 15), IsOngoing = false });

            model.Reports.Add(new ReportModel
            {
                Id = "R030",
                ReporterName = "Đặng Hải Yến",
                ReportedEntityId = UserId,
                ReportedEntityName = model.User.FullName,
                ReportedEntityType = ReportedEntityType.User,
                ViolationType = ViolationType.Other,
                Priority = ReportPriority.Low,
                Description = "Chậm phản hồi trong nhóm dự án chung, không có dấu hiệu vi phạm nghiêm trọng.",
                Status = ReportStatus.Dismissed,
                CreatedDate = DateTime.Now.AddMonths(-2),
                Resolution = new ResolutionModel
                {
                    Action = ResolutionAction.Dismiss,
                    Note = "Không phát hiện vi phạm, chỉ là hiểu lầm trong giao tiếp nhóm.",
                    ResolvedBy = "Admin",
                    ResolvedDate = DateTime.Now.AddMonths(-2).AddDays(1)
                }
            });

            model.AuditLogs.Add(new AuditLogEntryModel { Id = "UL1", EntityId = UserId, Action = "Đăng ký tài khoản", Description = "Tạo tài khoản mới với vai trò OSS Contributor.", PerformedBy = "System", Timestamp = model.User.JoinedDate });
            model.AuditLogs.Add(new AuditLogEntryModel { Id = "UL2", EntityId = UserId, Action = "Tham gia dự án", Description = "Tham gia dự án TaskMind Core Platform vai trò Technical leader.", PerformedBy = UserId, Timestamp = new DateTime(2025, 1, 10) });
            model.AuditLogs.Add(new AuditLogEntryModel { Id = "UL3", EntityId = UserId, Action = "Cập nhật kỹ năng", Description = "Thêm kỹ năng Docker vào hồ sơ cá nhân.", PerformedBy = UserId, Timestamp = new DateTime(2025, 8, 4) });

            Detail = model;
            IsBusy = false;
        }
    }
}