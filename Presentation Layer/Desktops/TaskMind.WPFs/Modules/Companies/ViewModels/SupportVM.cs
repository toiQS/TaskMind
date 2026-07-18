using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class SupportVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private SupportStatus? _statusFilter;
        public SupportStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private SupportType? _typeFilter;
        public SupportType? TypeFilter { get => _typeFilter; set { _typeFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private SupportRequestModel _selectedRequest;
        public SupportRequestModel SelectedRequest
        {
            get => _selectedRequest;
            set { _selectedRequest = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedRequest)); }
        }
        public bool HasSelectedRequest => SelectedRequest != null;

        public ObservableCollection<SupportRequestModel> Requests { get; } = new();
        public ObservableCollection<SupportRequestModel> FilteredRequests { get; } = new();

        public int PendingCount => Requests.Count(r => r.Status == SupportStatus.Pending);
        public int InProgressCount => Requests.Count(r => r.Status == SupportStatus.InProgress);
        public int CompletedCount => Requests.Count(r => r.Status == SupportStatus.Completed);

        public ICommand RefreshCommand { get; }
        public ICommand CreateRequestCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand CompleteCommand { get; }

        public SupportVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            CreateRequestCommand = new RelayCommand(_ => CreateRequest());
            OpenDetailCommand = new RelayCommand(p => SelectedRequest = p as SupportRequestModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedRequest = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; TypeFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is SupportStatus s ? s : (SupportStatus?)null);
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is SupportType t ? t : (SupportType?)null);
            ApproveCommand = new RelayCommand(p => UpdateStatus(p as SupportRequestModel, SupportStatus.Approved));
            RejectCommand = new RelayCommand(p => UpdateStatus(p as SupportRequestModel, SupportStatus.Rejected));
            CompleteCommand = new RelayCommand(p => UpdateStatus(p as SupportRequestModel, SupportStatus.Completed));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/support-requests thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Requests.Clear();
            Requests.Add(new SupportRequestModel
            {
                Title = "Nâng cấp RAM cho dàn máy dev",
                Description = "Đội Developer đang gặp tình trạng máy chậm khi build dự án lớn, đề xuất nâng cấp RAM 16GB → 32GB cho 8 máy.",
                Type = SupportType.DeviceUpgrade,
                Status = SupportStatus.Pending,
                Priority = SupportPriority.High,
                RequestedBy = "Lê Thị Hoa",
                Department = "Phòng Kỹ thuật",
                CreatedDate = DateTime.Now.AddDays(-2),
                EstimatedCost = 24_000_000m
            });
            Requests.Add(new SupportRequestModel
            {
                Title = "Tuyển thêm 2 Backend Developer",
                Description = "Dự án ERP nội bộ đang thiếu nhân sự backend, cần tuyển thêm để đảm bảo tiến độ Q3.",
                Type = SupportType.Hiring,
                Status = SupportStatus.InProgress,
                Priority = SupportPriority.Urgent,
                RequestedBy = "Trần Văn Bình",
                Department = "Phòng Kỹ thuật",
                CreatedDate = DateTime.Now.AddDays(-7)
            });
            Requests.Add(new SupportRequestModel
            {
                Title = "Gia hạn bản quyền JetBrains Rider",
                Description = "Bản quyền JetBrains Rider cho team hết hạn cuối tháng, cần mua gia hạn 10 seat.",
                Type = SupportType.LicensePurchase,
                Status = SupportStatus.Approved,
                Priority = SupportPriority.Medium,
                RequestedBy = "Phạm Minh Tuấn",
                Department = "Phòng Kỹ thuật",
                CreatedDate = DateTime.Now.AddDays(-10),
                EstimatedCost = 12_500_000m,
                AdminResponse = "Đã duyệt, kế toán sẽ thanh toán trong tuần này."
            });
            Requests.Add(new SupportRequestModel
            {
                Title = "Cập nhật môi trường CI/CD lên .NET 10",
                Description = "Nâng cấp pipeline CI/CD để hỗ trợ build/test dự án trên net10.0-windows.",
                Type = SupportType.EnvironmentUpdate,
                Status = SupportStatus.Completed,
                Priority = SupportPriority.Medium,
                RequestedBy = "Đỗ Thu Trang",
                Department = "Phòng Kỹ thuật",
                CreatedDate = DateTime.Now.AddMonths(-1),
                ResolvedDate = DateTime.Now.AddDays(-20),
                AdminResponse = "Đã hoàn tất nâng cấp pipeline."
            });
            Requests.Add(new SupportRequestModel
            {
                Title = "Ghế công thái học cho phòng dev",
                Description = "Một số nhân viên phản ánh đau lưng khi làm việc dài giờ, đề xuất mua thêm ghế.",
                Type = SupportType.Other,
                Status = SupportStatus.Rejected,
                Priority = SupportPriority.Low,
                RequestedBy = "Ngô Quốc Huy",
                Department = "Hành chính - Nhân sự",
                CreatedDate = DateTime.Now.AddDays(-15),
                ResolvedDate = DateTime.Now.AddDays(-12),
                AdminResponse = "Chưa đủ ngân sách quý này, xem xét lại vào quý sau."
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Requests.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(r =>
                    r.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    r.RequestedBy?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (StatusFilter.HasValue) query = query.Where(r => r.Status == StatusFilter.Value);
            if (TypeFilter.HasValue) query = query.Where(r => r.Type == TypeFilter.Value);

            FilteredRequests.Clear();
            foreach (var r in query.OrderByDescending(r => r.CreatedDate))
                FilteredRequests.Add(r);
        }

        private void UpdateStatus(SupportRequestModel request, SupportStatus status)
        {
            if (request == null) return;

            // TODO: gọi service PATCH /support-requests/{id}/status
            request.Status = status;
            if (status is SupportStatus.Completed or SupportStatus.Rejected or SupportStatus.Approved)
                request.ResolvedDate = DateTime.Now;

            Touch();
        }

        private void CreateRequest()
        {
            // TODO: mở dialog/điều hướng "Tạo yêu cầu hỗ trợ", gọi service POST /support-requests
        }

        /// <summary>Ép làm mới UI vì SupportRequestModel không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();
            if (SelectedRequest != null)
            {
                var updated = SelectedRequest;
                SelectedRequest = null;
                SelectedRequest = updated;
            }
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(InProgressCount));
            OnPropertyChanged(nameof(CompletedCount));
        }
    }
}