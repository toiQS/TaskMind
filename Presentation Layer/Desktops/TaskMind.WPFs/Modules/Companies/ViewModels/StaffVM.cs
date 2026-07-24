using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class StaffVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private StaffStatus? _statusFilter;
        public StaffStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private string _departmentFilter;
        public string DepartmentFilter { get => _departmentFilter; set { _departmentFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private bool _isAddingStaff;
        public bool IsAddingStaff { get => _isAddingStaff; set { _isAddingStaff = value; OnPropertyChanged(); } }

        private AddStaffVM _addStaffVM;
        public AddStaffVM AddStaffVM { get => _addStaffVM; set { _addStaffVM = value; OnPropertyChanged(); } }

        private StaffModel _selectedStaff;
        public StaffModel SelectedStaff
        {
            get => _selectedStaff;
            set { _selectedStaff = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedStaff)); }
        }
        public bool HasSelectedStaff => SelectedStaff != null;

        public ObservableCollection<StaffModel> Staffs { get; } = new();
        public ObservableCollection<StaffModel> FilteredStaffs { get; } = new();

        public int ActiveCount => Staffs.Count(s => s.Status == StaffStatus.Active);
        public int SuspendedCount => Staffs.Count(s => s.Status == StaffStatus.Suspended);
        public int ResignedCount => Staffs.Count(s => s.Status == StaffStatus.Resigned);

        public ICommand RefreshCommand { get; }
        public ICommand AddStaffCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand SuspendCommand { get; }
        public ICommand ActivateCommand { get; }
        public ICommand ResignCommand { get; }

        public StaffVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            AddStaffCommand = new RelayCommand(_ => AddStaff());
            OpenDetailCommand = new RelayCommand(p => SelectedStaff = p as StaffModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedStaff = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is StaffStatus s ? s : (StaffStatus?)null);
            SuspendCommand = new RelayCommand(p => UpdateStatus(p as StaffModel, StaffStatus.Suspended));
            ActivateCommand = new RelayCommand(p => UpdateStatus(p as StaffModel, StaffStatus.Active));
            ResignCommand = new RelayCommand(p => UpdateStatus(p as StaffModel, StaffStatus.Resigned));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/staffs thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Staffs.Clear();

            Staffs.Add(new StaffModel
            {
                FullName = "Lê Thị Hoa",
                Email = "hoalt@taskmind.vn",
                Phone = "0911 222 333",
                Position = "Technical Leader",
                Department = "Phòng Kỹ thuật",
                Status = StaffStatus.Active,
                JoinDate = DateTime.Now.AddYears(-2),
                Skills = new() { "C#", "ASP.NET Core", "Kiến trúc hệ thống" },
                ProjectNames = new() { "Hệ thống ERP nội bộ", "Website thương mại điện tử ABC" }
            });

            Staffs.Add(new StaffModel
            {
                FullName = "Trần Văn Bình",
                Email = "binhtv@taskmind.vn",
                Phone = "0912 333 444",
                Position = "Project Manager",
                Department = "Phòng Kỹ thuật",
                Status = StaffStatus.Active,
                JoinDate = DateTime.Now.AddYears(-3),
                Skills = new() { "Agile", "Project Planning" },
                ProjectNames = new() { "Hệ thống ERP nội bộ" }
            });

            Staffs.Add(new StaffModel
            {
                FullName = "Phạm Thị D",
                Email = "thid@taskmind.vn",
                Phone = "0904 567 890",
                Position = "QA/QC Intern",
                Department = "Phòng Kỹ thuật",
                Status = StaffStatus.Active,
                JoinDate = DateTime.Now.AddDays(-6),
                Skills = new() { "Manual Testing", "Test Case Design" },
                ProjectNames = new(),
                SourceCandidateName = "Phạm Thị D (ứng viên tin \"Thực tập sinh QA/QC\")"
            });

            Staffs.Add(new StaffModel
            {
                FullName = "Đỗ Thu Trang",
                Email = "trangdt@taskmind.vn",
                Phone = "0913 444 555",
                Position = "Backend Developer",
                Department = "Phòng Kỹ thuật",
                Status = StaffStatus.Suspended,
                JoinDate = DateTime.Now.AddMonths(-8),
                Skills = new() { "C#", "SQL Server" },
                ProjectNames = new() { "App quản lý kho" },
                Note = "Tạm ngưng theo yêu cầu cá nhân, nghỉ không lương 1 tháng."
            });

            Staffs.Add(new StaffModel
            {
                FullName = "Ngô Quốc Huy",
                Email = "huyngo@taskmind.vn",
                Phone = "0914 555 666",
                Position = "Warehouse System Owner",
                Department = "Phòng Vận hành",
                Status = StaffStatus.Resigned,
                JoinDate = DateTime.Now.AddYears(-4),
                LeftDate = DateTime.Now.AddDays(-15),
                Skills = new() { "ASP.NET Core", "Angular" },
                ProjectNames = new(),
                Note = "Đã nghỉ việc, bàn giao toàn bộ dự án App quản lý kho."
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Staffs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(s =>
                    s.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    s.Position?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    s.SkillsDisplay.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (StatusFilter.HasValue) query = query.Where(s => s.Status == StatusFilter.Value);
            if (!string.IsNullOrWhiteSpace(DepartmentFilter)) query = query.Where(s => s.Department == DepartmentFilter);

            FilteredStaffs.Clear();
            foreach (var s in query.OrderBy(s => s.FullName))
                FilteredStaffs.Add(s);
        }

        private void UpdateStatus(StaffModel staff, StaffStatus status)
        {
            if (staff == null) return;

            // TODO: gọi service PATCH /staffs/{id}/status
            staff.Status = status;
            staff.LeftDate = status == StaffStatus.Resigned ? DateTime.Now : staff.LeftDate;

            Touch();
        }

        private void AddStaff()
        {
            SelectedStaff = null; // đóng panel chi tiết nếu đang mở, tránh chồng 2 overlay

            var vm = new AddStaffVM();
            vm.OnSaved = staff =>
            {
                // TODO: khi có service thật, có thể gọi lại LoadAsync() thay vì chèn trực tiếp vào danh sách cục bộ
                Staffs.Add(staff);
                ApplyFilter();
                RaiseCounters();

                IsAddingStaff = false;
                AddStaffVM = null;
            };
            vm.OnCancelled = () =>
            {
                IsAddingStaff = false;
                AddStaffVM = null;
            };

            AddStaffVM = vm;
            IsAddingStaff = true;
        }

        /// <summary>Ép làm mới UI vì StaffModel không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();
            if (SelectedStaff != null)
            {
                var updated = SelectedStaff;
                SelectedStaff = null;
                SelectedStaff = updated;
            }
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(ActiveCount));
            OnPropertyChanged(nameof(SuspendedCount));
            OnPropertyChanged(nameof(ResignedCount));
        }
    }
}