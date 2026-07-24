using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class HandlerVM : ViewModelBase
    {
        public ObservableCollection<ReportModel> Reports { get; } = new ObservableCollection<ReportModel>();

        private ICollectionView _reportsView;
        public ICollectionView ReportsView
        {
            get => _reportsView;
            private set { _reportsView = value; OnPropertyChanged(); }
        }

        /// <summary>Toàn bộ audit log của hệ thống, lọc theo entity của report đang chọn</summary>
        public ObservableCollection<AuditLogEntryModel> AuditLogs { get; } = new ObservableCollection<AuditLogEntryModel>();

        private ICollectionView _auditLogView;
        public ICollectionView AuditLogView
        {
            get => _auditLogView;
            private set { _auditLogView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ReportsView?.Refresh(); }
        }

        /// <summary>"All" | tên ReportStatus</summary>
        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); ReportsView?.Refresh(); }
        }

        private ReportModel _selectedReport;
        public ReportModel SelectedReport
        {
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedReport));
                OnPropertyChanged(nameof(CanResolve));

                // reset form xử lý mỗi khi chọn report khác
                ResolutionNote = value?.Resolution?.Note ?? string.Empty;
                SelectedResolutionAction = value?.Resolution?.Action ?? ResolutionAction.Warning;

                AuditLogView?.Refresh();
            }
        }

        public bool HasSelectedReport => SelectedReport != null;
        public bool CanResolve => SelectedReport != null &&
                                   SelectedReport.Status != ReportStatus.Resolved &&
                                   SelectedReport.Status != ReportStatus.Dismissed;

        public Array ResolutionActionOptions => Enum.GetValues(typeof(ResolutionAction));

        private ResolutionAction _selectedResolutionAction = ResolutionAction.Warning;
        public ResolutionAction SelectedResolutionAction
        {
            get => _selectedResolutionAction;
            set { _selectedResolutionAction = value; OnPropertyChanged(); }
        }

        private string _resolutionNote;
        public string ResolutionNote
        {
            get => _resolutionNote;
            set { _resolutionNote = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterStatusCommand { get; }
        public ICommand StartReviewCommand { get; }
        public ICommand SubmitResolutionCommand { get; }
        public ICommand DismissCommand { get; }

        public HandlerVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterStatusCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            StartReviewCommand = new RelayCommand(_ => StartReview());
            SubmitResolutionCommand = new RelayCommand(_ => SubmitResolution());
            DismissCommand = new RelayCommand(_ => Dismiss());

            ReportsView = CollectionViewSource.GetDefaultView(Reports);
            ReportsView.Filter = FilterReports;
            ReportsView.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                nameof(ReportModel.CreatedDate), System.ComponentModel.ListSortDirection.Descending));

            AuditLogView = new ListCollectionView(AuditLogs) { Filter = FilterAuditLogs };
            AuditLogView.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                nameof(AuditLogEntryModel.Timestamp), System.ComponentModel.ListSortDirection.Descending));

            _ = LoadDataAsync();
        }

        private bool FilterReports(object obj)
        {
            if (obj is not ReportModel r) return false;

            if (StatusFilter != "All" &&
                !string.Equals(r.Status.ToString(), StatusFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                r.ReportedEntityName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0 &&
                r.ReporterName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private bool FilterAuditLogs(object obj)
        {
            if (obj is not AuditLogEntryModel log || SelectedReport == null) return false;
            return string.Equals(log.EntityId, SelectedReport.ReportedEntityId, StringComparison.OrdinalIgnoreCase);
        }

        private void StartReview()
        {
            if (SelectedReport == null || SelectedReport.Status != ReportStatus.Pending) return;

            SelectedReport.Status = ReportStatus.Reviewing;
            // TODO: gọi service PUT /reports/{id}/start-review
            Touch(SelectedReport);
        }

        private void SubmitResolution()
        {
            if (SelectedReport == null || string.IsNullOrWhiteSpace(ResolutionNote)) return;

            SelectedReport.Resolution = new ResolutionModel
            {
                Action = SelectedResolutionAction,
                Note = ResolutionNote.Trim(),
                ResolvedBy = "Admin", // TODO: lấy từ phiên đăng nhập hiện tại
                ResolvedDate = DateTime.Now
            };
            SelectedReport.Status = ReportStatus.Resolved;

            // TODO: gọi service POST /reports/{id}/resolve — ghi nhận resolution vào hệ thống
            // TODO: tuỳ SelectedResolutionAction, gọi thêm service tương ứng:
            //   Warning          -> gửi cảnh báo (Notification)
            //   LockAccount      -> PUT /users/{id}/lock
            //   BanAccount       -> PUT /users/{id}/ban
            //   SuspendOrganization -> PUT /companies|schools/{id}/suspend

            AppendAuditLog(SelectedReport, $"Xử lý báo cáo: {SelectedResolutionAction}", ResolutionNote);
            Touch(SelectedReport);
        }

        private void Dismiss()
        {
            if (SelectedReport == null) return;

            SelectedReport.Resolution = new ResolutionModel
            {
                Action = ResolutionAction.Dismiss,
                Note = string.IsNullOrWhiteSpace(ResolutionNote) ? "Không phát hiện vi phạm." : ResolutionNote.Trim(),
                ResolvedBy = "Admin",
                ResolvedDate = DateTime.Now
            };
            SelectedReport.Status = ReportStatus.Dismissed;

            // TODO: gọi service POST /reports/{id}/dismiss
            AppendAuditLog(SelectedReport, "Bỏ qua báo cáo", SelectedReport.Resolution.Note);
            Touch(SelectedReport);
        }

        private void AppendAuditLog(ReportModel report, string action, string description)
        {
            AuditLogs.Add(new AuditLogEntryModel
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                EntityId = report.ReportedEntityId,
                Action = action,
                Description = description,
                PerformedBy = "Admin",
                Timestamp = DateTime.Now
            });
            AuditLogView.Refresh();
        }

        /// <summary>ReportModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item để UI + filter cập nhật.</summary>
        private void Touch(ReportModel changed)
        {
            int index = Reports.IndexOf(changed);
            if (index >= 0)
            {
                Reports.RemoveAt(index);
                Reports.Insert(index, changed);
                SelectedReport = changed;
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy danh sách báo cáo vi phạm + audit log toàn hệ thống.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Reports.Clear();
            AuditLogs.Clear();

            Reports.Add(new ReportModel
            {
                Id = "R001",
                ReporterName = "Đặng Hải Yến",
                ReportedEntityId = "U006",
                ReportedEntityName = "Vũ Đức Anh",
                ReportedEntityType = ReportedEntityType.User,
                ViolationType = ViolationType.SpamContent,
                Priority = ReportPriority.High,
                Description = "Tài khoản liên tục đăng liên kết quảng cáo spam trong phần bình luận dự án.",
                Status = ReportStatus.Pending,
                CreatedDate = DateTime.Now.AddHours(-5)
            });

            Reports.Add(new ReportModel
            {
                Id = "R002",
                ReporterName = "Lê Minh Khoa",
                ReportedEntityId = "C004",
                ReportedEntityName = "Vietsoft Solutions",
                ReportedEntityType = ReportedEntityType.Company,
                ViolationType = ViolationType.FraudPayment,
                Priority = ReportPriority.High,
                Description = "Công ty không thanh toán milestone đã hoàn thành cho freelancer sau 30 ngày.",
                Status = ReportStatus.Reviewing,
                CreatedDate = DateTime.Now.AddDays(-2)
            });

            Reports.Add(new ReportModel
            {
                Id = "R003",
                ReporterName = "Trần Thị Bích",
                ReportedEntityId = "U004",
                ReportedEntityName = "Ngô Thanh Tùng",
                ReportedEntityType = ReportedEntityType.User,
                ViolationType = ViolationType.Harassment,
                Priority = ReportPriority.Medium,
                Description = "Có lời lẽ xúc phạm thành viên khác trong nhóm dự án.",
                Status = ReportStatus.Pending,
                CreatedDate = DateTime.Now.AddDays(-1)
            });

            Reports.Add(new ReportModel
            {
                Id = "R004",
                ReporterName = "Phạm Gia Huy",
                ReportedEntityId = "S004",
                ReportedEntityName = "Trung tâm Tin học XYZ",
                ReportedEntityType = ReportedEntityType.School,
                ViolationType = ViolationType.FakeInformation,
                Priority = ReportPriority.Medium,
                Description = "Quảng cáo sai lệch về đội ngũ giảng viên và tỉ lệ có việc làm sau khoá học.",
                Status = ReportStatus.Resolved,
                CreatedDate = DateTime.Now.AddDays(-6),
                Resolution = new ResolutionModel
                {
                    Action = ResolutionAction.SuspendOrganization,
                    Note = "Xác minh có sai lệch thông tin, tạm ngưng hoạt động 14 ngày để chỉnh sửa.",
                    ResolvedBy = "Admin",
                    ResolvedDate = DateTime.Now.AddDays(-5)
                }
            });

            // ----- Audit log mẫu, liên kết theo EntityId -----
            AuditLogs.Add(new AuditLogEntryModel { Id = "L1", EntityId = "U006", Action = "Đăng ký tài khoản", Description = "Tạo tài khoản mới.", PerformedBy = "System", Timestamp = DateTime.Now.AddDays(-40) });
            AuditLogs.Add(new AuditLogEntryModel { Id = "L2", EntityId = "U006", Action = "Cảnh cáo", Description = "Cảnh cáo lần 1 do đăng nội dung không phù hợp.", PerformedBy = "Admin", Timestamp = DateTime.Now.AddDays(-10) });
            AuditLogs.Add(new AuditLogEntryModel { Id = "L3", EntityId = "U006", Action = "Đăng bình luận", Description = "Đăng liên kết ngoài trong bình luận dự án #P312.", PerformedBy = "U006", Timestamp = DateTime.Now.AddHours(-6) });

            AuditLogs.Add(new AuditLogEntryModel { Id = "L4", EntityId = "C004", Action = "Duyệt công ty", Description = "Công ty được duyệt tham gia hệ thống.", PerformedBy = "Admin", Timestamp = DateTime.Now.AddMonths(-8) });
            AuditLogs.Add(new AuditLogEntryModel { Id = "L5", EntityId = "C004", Action = "Giao dịch trao đổi", Description = "Milestone #M22 được đánh dấu hoàn thành nhưng chưa thanh toán.", PerformedBy = "C004", Timestamp = DateTime.Now.AddDays(-3) });

            AuditLogs.Add(new AuditLogEntryModel { Id = "L6", EntityId = "U004", Action = "Tham gia dự án", Description = "Tham gia dự án #P108 vai trò Developer.", PerformedBy = "U004", Timestamp = DateTime.Now.AddDays(-20) });
            AuditLogs.Add(new AuditLogEntryModel { Id = "L7", EntityId = "U004", Action = "Bình luận bị báo cáo", Description = "Bình luận trong task #T55 bị báo cáo bởi thành viên nhóm.", PerformedBy = "U004", Timestamp = DateTime.Now.AddDays(-1) });

            AuditLogs.Add(new AuditLogEntryModel { Id = "L8", EntityId = "S004", Action = "Duyệt cơ sở đào tạo", Description = "Cơ sở được duyệt tham gia hệ thống.", PerformedBy = "Admin", Timestamp = DateTime.Now.AddMonths(-20) });
            AuditLogs.Add(new AuditLogEntryModel { Id = "L9", EntityId = "S004", Action = "Tạm ngưng", Description = "Tạm ngưng hoạt động 14 ngày sau khi xác minh vi phạm báo cáo #R004.", PerformedBy = "Admin", Timestamp = DateTime.Now.AddDays(-5) });

            SelectedReport = Reports.FirstOrDefault(r => r.Status == ReportStatus.Pending);

            IsBusy = false;
        }
    }
}