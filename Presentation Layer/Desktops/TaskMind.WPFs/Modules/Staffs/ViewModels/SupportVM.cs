using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class SupportVM : ViewModelBase
    {
        // TODO: thay bằng tên nhân sự đang đăng nhập lấy từ phiên làm việc thực tế.
        private const string CurrentUserName = "Lê Thị Hoa";

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private SupportStatus? _statusFilter;
        public SupportStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private SupportRequestModel _selectedRequest;
        public SupportRequestModel SelectedRequest
        {
            get => _selectedRequest;
            set { _selectedRequest = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedRequest)); }
        }
        public bool HasSelectedRequest => SelectedRequest != null;

        private string _messageInput;
        public string MessageInput { get => _messageInput; set { _messageInput = value; OnPropertyChanged(); } }

        /// <summary>True khi panel "Tạo yêu cầu hỗ trợ" đang mở (overlay).</summary>
        private bool _isCreating;
        public bool IsCreating { get => _isCreating; set { _isCreating = value; OnPropertyChanged(); } }

        private CreateSupportVM _createSupportVM;
        public CreateSupportVM CreateSupportVM { get => _createSupportVM; set { _createSupportVM = value; OnPropertyChanged(); } }

        /// <summary>Toàn bộ yêu cầu hỗ trợ cá nhân đã gửi đến Admin công ty.</summary>
        public ObservableCollection<SupportRequestModel> Requests { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc, sắp xếp theo hoạt động gần nhất.</summary>
        public ObservableCollection<SupportRequestModel> FilteredRequests { get; } = new();

        // ===== Thống kê =====
        public int TotalCount => Requests.Count;
        public int PendingCount => Requests.Count(r => r.Status == SupportStatus.Pending);
        public int InProgressCount => Requests.Count(r => r.Status == SupportStatus.InProgress);
        public int ResolvedCount => Requests.Count(r => r.Status == SupportStatus.Resolved);

        public ICommand RefreshCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand AddMessageCommand { get; }
        public ICommand CloseRequestCommand { get; }
        public ICommand CreateRequestCommand { get; }

        public SupportVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is SupportStatus s ? s : (SupportStatus?)null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; });
            OpenDetailCommand = new RelayCommand(p => SelectedRequest = p as SupportRequestModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedRequest = null);
            AddMessageCommand = new RelayCommand(_ => AddMessage(), _ => CanAddMessage());
            CloseRequestCommand = new RelayCommand(p => CloseRequest(p as SupportRequestModel));
            CreateRequestCommand = new RelayCommand(_ => CreateRequest());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/support-requests thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            Requests.Clear();

            var r1 = new SupportRequestModel
            {
                Title = "Không đăng nhập được sau khi đổi mật khẩu",
                Description = "Tôi đã đổi mật khẩu theo hướng dẫn nhưng hệ thống báo sai tài khoản khi đăng nhập lại.",
                Category = SupportCategory.Account,
                Priority = TodoPriority.Urgent,
                Status = SupportStatus.InProgress,
                CreatedDate = DateTime.Now.AddDays(-2),
                AdminName = "Nguyễn Văn Admin"
            };
            r1.Replies.Add(new SupportReplyModel { Author = CurrentUserName, Content = "Tôi đã thử trên cả điện thoại và máy tính, vẫn báo lỗi tương tự.", CreatedDate = DateTime.Now.AddDays(-2).AddHours(1), IsFromStaff = true });
            r1.Replies.Add(new SupportReplyModel { Author = "Nguyễn Văn Admin", Content = "Bên mình đang kiểm tra lại log đăng nhập, sẽ phản hồi trong hôm nay.", CreatedDate = DateTime.Now.AddDays(-1), IsFromStaff = false });
            Requests.Add(r1);

            var r2 = new SupportRequestModel
            {
                Title = "Thắc mắc về bảng lương tháng trước",
                Description = "Số ngày công trên bảng lương tháng trước có vẻ chưa khớp với thực tế chấm công của tôi.",
                Category = SupportCategory.Salary,
                Priority = TodoPriority.Medium,
                Status = SupportStatus.Resolved,
                CreatedDate = DateTime.Now.AddDays(-10),
                ResolvedDate = DateTime.Now.AddDays(-8),
                AdminName = "Trần Thị Kế Toán"
            };
            r2.Replies.Add(new SupportReplyModel { Author = "Trần Thị Kế Toán", Content = "Đã đối chiếu lại và điều chỉnh, phần chênh lệch sẽ cộng vào kỳ lương tới.", CreatedDate = DateTime.Now.AddDays(-8), IsFromStaff = false });
            Requests.Add(r2);

            var r3 = new SupportRequestModel
            {
                Title = "Không truy cập được tài liệu dự án ERP",
                Description = "Thư mục tài liệu thiết kế của dự án ERP nội bộ báo lỗi không có quyền truy cập.",
                Category = SupportCategory.Project,
                Priority = TodoPriority.High,
                Status = SupportStatus.Pending,
                CreatedDate = DateTime.Now.AddHours(-5)
            };
            Requests.Add(r3);

            var r4 = new SupportRequestModel
            {
                Title = "Đề xuất cấp thêm license phần mềm thiết kế",
                Description = "Công việc hiện tại cần dùng thêm license phần mềm thiết kế UI, mong Admin hỗ trợ cấp phát.",
                Category = SupportCategory.Other,
                Priority = TodoPriority.Low,
                Status = SupportStatus.Closed,
                CreatedDate = DateTime.Now.AddDays(-20),
                ResolvedDate = DateTime.Now.AddDays(-18),
                AdminName = "Nguyễn Văn Admin"
            };
            Requests.Add(r4);

            ApplyFilter();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Requests.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(r => r.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (StatusFilter.HasValue)
                query = query.Where(r => r.Status == StatusFilter.Value);

            FilteredRequests.Clear();
            foreach (var r in query.OrderByDescending(r => r.LastActivityDate))
                FilteredRequests.Add(r);

            RaiseCounters();
        }

        private bool CanAddMessage() => SelectedRequest != null && SelectedRequest.IsOpen && !string.IsNullOrWhiteSpace(MessageInput);

        private void AddMessage()
        {
            if (!CanAddMessage()) return;

            // TODO: gọi service POST /me/support-requests/{id}/messages
            SelectedRequest.Replies.Add(new SupportReplyModel
            {
                Author = CurrentUserName,
                Content = MessageInput.Trim(),
                IsFromStaff = true
            });

            MessageInput = string.Empty;
            Touch();
        }

        /// <summary>Nhân sự tự đóng yêu cầu sau khi xác nhận Admin đã giải quyết xong.</summary>
        private void CloseRequest(SupportRequestModel request)
        {
            if (request == null || !request.CanClose) return;

            // TODO: gọi service PATCH /me/support-requests/{id}/close
            request.Status = SupportStatus.Closed;
            Touch();
        }

        /// <summary>Mở panel "Tạo yêu cầu hỗ trợ mới", gán callback nhận SupportRequestModel vừa tạo.</summary>
        private void CreateRequest()
        {
            SelectedRequest = null; // đóng panel chi tiết nếu đang mở, tránh chồng 2 overlay

            var vm = new CreateSupportVM(CurrentUserName);

            vm.OnSaved = request =>
            {
                Requests.Insert(0, request);
                ApplyFilter();

                IsCreating = false;
                CreateSupportVM = null;
            };
            vm.OnCancelled = () =>
            {
                IsCreating = false;
                CreateSupportVM = null;
            };

            CreateSupportVM = vm;
            IsCreating = true;
        }

        /// <summary>Ép làm mới UI vì SupportRequestModel không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();

            if (SelectedRequest != null)
            {
                var updated = SelectedRequest;
                SelectedRequest = null;
                SelectedRequest = updated;
            }
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(InProgressCount));
            OnPropertyChanged(nameof(ResolvedCount));
        }
    }
}