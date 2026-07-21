using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class AddProjectVM : ViewModelBase
    {
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private ProjectKind _kind = ProjectKind.Internal;
        public ProjectKind Kind { get => _kind; set { _kind = value; OnPropertyChanged(); } }

        private DateTime _startDate = DateTime.Now;
        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

        private DateTime? _endDate;
        public DateTime? EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        private string _partnerName;
        public string PartnerName { get => _partnerName; set { _partnerName = value; OnPropertyChanged(); } }

        private decimal? _contractValue;
        public decimal? ContractValue { get => _contractValue; set { _contractValue = value; OnPropertyChanged(); } }

        // ===== Chọn thành viên: luôn từ nhân sự trực thuộc công ty (mục 4.5) =====

        private string _staffSearchText;
        public string StaffSearchText
        {
            get => _staffSearchText;
            set { _staffSearchText = value; OnPropertyChanged(); ApplyStaffFilter(); }
        }

        private ProjectRole _memberRoleInput = ProjectRole.Developer;
        /// <summary>Vai trò sẽ được gán khi bấm chọn 1 nhân sự trong danh sách picker.</summary>
        public ProjectRole MemberRoleInput
        {
            get => _memberRoleInput;
            set { _memberRoleInput = value; OnPropertyChanged(); }
        }

        private bool _isLoadingStaff;
        public bool IsLoadingStaff { get => _isLoadingStaff; set { _isLoadingStaff = value; OnPropertyChanged(); } }

        /// <summary>Toàn bộ nhân sự đang hoạt động của công ty (nguồn: StaffVM.Staffs lọc StaffStatus.Active).</summary>
        public ObservableCollection<ProjectStaffOption> AvailableStaff { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm, dùng để bind lên View.</summary>
        public ObservableCollection<ProjectStaffOption> FilteredAvailableStaff { get; } = new();

        /// <summary>Danh sách thành viên đã chọn cho dự án — mỗi phần tử luôn tham chiếu 1 StaffId có thật.</summary>
        public ObservableCollection<ProjectMemberModel> Members { get; } = new();

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand SetKindCommand { get; }
        public ICommand ToggleStaffMemberCommand { get; }
        public ICommand RemoveMemberCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>ProjectVM gán 2 callback này khi mở panel, để nhận ProjectModel vừa tạo hoặc đóng panel khi huỷ.</summary>
        public Action<ProjectModel> OnSaved { get; set; }
        public Action OnCancelled { get; set; }

        public AddProjectVM()
        {
            SetKindCommand = new RelayCommand(p => Kind = p is ProjectKind k ? k : ProjectKind.Internal);
            ToggleStaffMemberCommand = new RelayCommand(p => ToggleStaffMember(p as ProjectStaffOption));
            RemoveMemberCommand = new RelayCommand(p => RemoveMember(p as ProjectMemberModel));
            CreateCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());

            _ = LoadAvailableStaffAsync();
        }

        private async Task LoadAvailableStaffAsync()
        {
            IsLoadingStaff = true;

            // TODO: gọi service GET /company/{companyId}/staffs?status=Active thay cho dữ liệu mẫu bên dưới
            // (cùng nguồn dữ liệu với StaffVM.Staffs — chỉ nhân sự StaffStatus.Active mới được gán vào
            // dự án mới; nhân sự tạm ngưng/đã rời công ty không hiển thị ở đây, liên kết mục 4.5 + 4.7).
            await Task.Delay(300);

            AvailableStaff.Clear();
            AvailableStaff.Add(new ProjectStaffOption { StaffId = Guid.NewGuid(), FullName = "Lê Thị Hoa", Position = "Technical Leader", Department = "Phòng Kỹ thuật", Skills = new() { "C#", "ASP.NET Core", "Kiến trúc hệ thống" } });
            AvailableStaff.Add(new ProjectStaffOption { StaffId = Guid.NewGuid(), FullName = "Trần Văn Bình", Position = "Project Manager", Department = "Phòng Kỹ thuật", Skills = new() { "Agile", "Project Planning" } });
            AvailableStaff.Add(new ProjectStaffOption { StaffId = Guid.NewGuid(), FullName = "Phạm Thị D", Position = "QA/QC Intern", Department = "Phòng Kỹ thuật", Skills = new() { "Manual Testing", "Test Case Design" } });
            AvailableStaff.Add(new ProjectStaffOption { StaffId = Guid.NewGuid(), FullName = "Phạm Minh Tuấn", Position = "Backend Developer", Department = "Phòng Kỹ thuật", Skills = new() { "C#", "SQL Server" } });
            AvailableStaff.Add(new ProjectStaffOption { StaffId = Guid.NewGuid(), FullName = "Đỗ Thu Trang", Position = "Backend Developer", Department = "Phòng Kỹ thuật", Skills = new() { "C#", "SQL Server", "REST API" } });

            ApplyStaffFilter();
            IsLoadingStaff = false;
        }

        private void ApplyStaffFilter()
        {
            var query = AvailableStaff.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(StaffSearchText))
                query = query.Where(s =>
                    s.FullName?.Contains(StaffSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    s.Position?.Contains(StaffSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    s.SkillsDisplay.Contains(StaffSearchText, StringComparison.OrdinalIgnoreCase));

            FilteredAvailableStaff.Clear();
            foreach (var s in query.OrderBy(s => s.FullName))
                FilteredAvailableStaff.Add(s);
        }

        /// <summary>Bấm vào 1 nhân sự trong picker: thêm vào dự án với vai trò đang chọn, hoặc gỡ ra nếu đã có.</summary>
        private void ToggleStaffMember(ProjectStaffOption option)
        {
            if (option == null) return;

            if (option.IsAdded)
            {
                var existing = Members.FirstOrDefault(m => m.StaffId == option.StaffId);
                if (existing != null) Members.Remove(existing);

                option.IsAdded = false;
            }
            else
            {
                Members.Add(new ProjectMemberModel
                {
                    StaffId = option.StaffId,
                    FullName = option.FullName,
                    Role = MemberRoleInput
                });

                option.IsAdded = true;
            }

            RefreshStaffList();
        }

        private void RemoveMember(ProjectMemberModel member)
        {
            if (member == null) return;

            Members.Remove(member);

            var option = AvailableStaff.FirstOrDefault(s => s.StaffId == member.StaffId);
            if (option != null) option.IsAdded = false;

            RefreshStaffList();
        }

        /// <summary>Ép ItemsControl render lại container để DataTrigger đọc lại IsAdded mới nhất
        /// (ProjectStaffOption không implement INotifyPropertyChanged).</summary>
        private void RefreshStaffList()
        {
            var current = FilteredAvailableStaff.ToList();
            FilteredAvailableStaff.Clear();
            foreach (var s in current)
                FilteredAvailableStaff.Add(s);
        }

        private bool Validate()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Vui lòng nhập tên dự án.";
                return false;
            }

            if (Kind == ProjectKind.Exchange && string.IsNullOrWhiteSpace(PartnerName))
            {
                ErrorMessage = "Dự án trao đổi cần nhập tên đối tác.";
                return false;
            }

            if (EndDate.HasValue && EndDate.Value.Date < StartDate.Date)
            {
                ErrorMessage = "Ngày kết thúc không được trước ngày bắt đầu.";
                return false;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;

            var project = new ProjectModel
            {
                Name = Name.Trim(),
                Description = Description?.Trim(),
                Status = ProjectStatus.InProgress,
                Kind = Kind,
                StartDate = StartDate,
                EndDate = EndDate,
                Progress = 0,
                TaskTotal = 0,
                TaskDone = 0,
                PartnerName = Kind == ProjectKind.Exchange ? PartnerName?.Trim() : null,
                ContractValue = Kind == ProjectKind.Exchange ? ContractValue : null
            };

            foreach (var m in Members)
                project.Members.Add(m);

            // TODO: gọi service POST /company/{companyId}/projects thay cho việc thêm trực tiếp vào danh sách cục bộ ở ProjectVM
            await Task.Delay(400);

            IsBusy = false;
            OnSaved?.Invoke(project);
        }
    }
}