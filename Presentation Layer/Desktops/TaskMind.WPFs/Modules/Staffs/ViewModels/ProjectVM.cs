using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class ProjectVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private ProjectStatus? _statusFilter;
        public ProjectStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private ProjectModel _selectedProject;
        public ProjectModel SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedProject)); }
        }
        public bool HasSelectedProject => SelectedProject != null;

        /// <summary>Toàn bộ dự án nhân sự đã/đang tham gia, tải từ service.</summary>
        public ObservableCollection<ProjectModel> Projects { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc, dùng để bind lên View.</summary>
        public ObservableCollection<ProjectModel> FilteredProjects { get; } = new();

        public int OngoingCount => Projects.Count(p => p.IsOngoing);
        public int CompletedCount => Projects.Count(p => p.Status == ProjectStatus.Completed);
        public int TotalCount => Projects.Count;

        public ICommand RefreshCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }

        public ProjectVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            OpenDetailCommand = new RelayCommand(p => SelectedProject = p as ProjectModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedProject = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is ProjectStatus s ? s : (ProjectStatus?)null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/projects (các dự án mà nhân sự hiện tại là thành viên) thay cho
            // dữ liệu mẫu bên dưới. Backend nên trả kèm MyRole/MyJoinedDate/MyTaskTotal/MyTaskDone dựa
            // trên bản ghi thành viên của chính nhân sự trong từng dự án (khác Progress/TaskTotal tổng thể).
            await Task.Delay(400);

            Projects.Clear();

            var p1 = new ProjectModel
            {
                Name = "Hệ thống ERP nội bộ",
                Description = "Xây dựng phân hệ quản lý nhân sự và chấm công cho toàn công ty.",
                Status = ProjectStatus.InProgress,
                Kind = ProjectKind.Internal,
                StartDate = DateTime.Now.AddMonths(-2),
                Progress = 62,
                TaskTotal = 48,
                TaskDone = 30,
                MyRole = ProjectRole.TechnicalLeader,
                MyJoinedDate = DateTime.Now.AddMonths(-2),
                MyTaskTotal = 10,
                MyTaskDone = 7
            };
            p1.Members.Add(new ProjectMemberModel { FullName = "Trần Văn Bình", Role = ProjectRole.Owner });
            p1.Members.Add(new ProjectMemberModel { FullName = "Lê Thị Hoa", Role = ProjectRole.TechnicalLeader, IsMe = true });
            p1.Members.Add(new ProjectMemberModel { FullName = "Nguyễn Văn A", Role = ProjectRole.Developer });
            Projects.Add(p1);

            var p2 = new ProjectModel
            {
                Name = "Website thương mại điện tử ABC",
                Description = "Dự án trao đổi với đối tác ABC Corp, thanh toán theo cột mốc (milestone).",
                Status = ProjectStatus.InProgress,
                Kind = ProjectKind.Exchange,
                PartnerName = "ABC Corp",
                ContractValue = 350_000_000m,
                StartDate = DateTime.Now.AddMonths(-1),
                Progress = 35,
                TaskTotal = 60,
                TaskDone = 21,
                MyRole = ProjectRole.Developer,
                MyJoinedDate = DateTime.Now.AddMonths(-1),
                MyTaskTotal = 8,
                MyTaskDone = 3
            };
            p2.Members.Add(new ProjectMemberModel { FullName = "Phạm Minh Tuấn", Role = ProjectRole.ProjectManager });
            p2.Members.Add(new ProjectMemberModel { FullName = "Đỗ Thu Trang", Role = ProjectRole.QaQc });
            p2.Members.Add(new ProjectMemberModel { FullName = "Lê Thị Hoa", Role = ProjectRole.Developer, IsMe = true });
            Projects.Add(p2);

            var p3 = new ProjectModel
            {
                Name = "App quản lý kho",
                Description = "Đã bàn giao cho khách hàng, hiện đang trong giai đoạn bảo trì.",
                Status = ProjectStatus.Completed,
                Kind = ProjectKind.Exchange,
                PartnerName = "Kho Vận Miền Nam",
                StartDate = DateTime.Now.AddMonths(-6),
                EndDate = DateTime.Now.AddDays(-10),
                Progress = 100,
                TaskTotal = 80,
                TaskDone = 80,
                MyRole = ProjectRole.Developer,
                MyJoinedDate = DateTime.Now.AddMonths(-6),
                MyTaskTotal = 14,
                MyTaskDone = 14
            };
            p3.Members.Add(new ProjectMemberModel { FullName = "Ngô Quốc Huy", Role = ProjectRole.Owner });
            p3.Members.Add(new ProjectMemberModel { FullName = "Lê Thị Hoa", Role = ProjectRole.Developer, IsMe = true });
            Projects.Add(p3);

            var p4 = new ProjectModel
            {
                Name = "Nền tảng học trực tuyến",
                Description = "Tạm dừng do chờ ký lại hợp đồng với cơ sở đào tạo.",
                Status = ProjectStatus.Paused,
                Kind = ProjectKind.Internal,
                StartDate = DateTime.Now.AddMonths(-3),
                Progress = 45,
                TaskTotal = 40,
                TaskDone = 18,
                MyRole = ProjectRole.Intern,
                MyJoinedDate = DateTime.Now.AddMonths(-3),
                MyTaskTotal = 5,
                MyTaskDone = 2
            };
            p4.Members.Add(new ProjectMemberModel { FullName = "Vũ Thị Mai", Role = ProjectRole.ProjectManager });
            p4.Members.Add(new ProjectMemberModel { FullName = "Lê Thị Hoa", Role = ProjectRole.Intern, IsMe = true });
            Projects.Add(p4);

            var p5 = new ProjectModel
            {
                Name = "Thư viện xử lý ảnh nội bộ",
                Description = "Dự án bị huỷ giữa chừng do đổi hướng công nghệ.",
                Status = ProjectStatus.Cancelled,
                Kind = ProjectKind.Internal,
                StartDate = DateTime.Now.AddMonths(-10),
                EndDate = DateTime.Now.AddMonths(-8),
                Progress = 20,
                TaskTotal = 25,
                TaskDone = 5,
                MyRole = ProjectRole.Developer,
                MyJoinedDate = DateTime.Now.AddMonths(-10),
                MyTaskTotal = 4,
                MyTaskDone = 1
            };
            p5.Members.Add(new ProjectMemberModel { FullName = "Lê Thị Hoa", Role = ProjectRole.Developer, IsMe = true });
            Projects.Add(p5);

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Projects.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(p => p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (StatusFilter.HasValue)
                query = query.Where(p => p.Status == StatusFilter.Value);

            FilteredProjects.Clear();
            foreach (var p in query.OrderByDescending(p => p.IsOngoing).ThenByDescending(p => p.StartDate))
                FilteredProjects.Add(p);
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(OngoingCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(TotalCount));
        }
    }
}