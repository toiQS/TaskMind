using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class ProjectVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private ProjectStatus? _statusFilter;
        public ProjectStatus? StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private ProjectModel _selectedProject;
        public ProjectModel SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedProject)); }
        }

        public bool HasSelectedProject => SelectedProject != null;

        /// <summary>True khi panel "Tạo dự án mới" đang mở (overlay ở ProjectView).</summary>
        private bool _isAddingProject;
        public bool IsAddingProject
        {
            get => _isAddingProject;
            set { _isAddingProject = value; OnPropertyChanged(); }
        }

        /// <summary>ViewModel của form tạo dự án, được tạo mới mỗi lần mở panel.</summary>
        private AddProjectVM _addProjectVM;
        public AddProjectVM AddProjectVM
        {
            get => _addProjectVM;
            set { _addProjectVM = value; OnPropertyChanged(); }
        }

        /// <summary>Toàn bộ dự án tải từ service.</summary>
        public ObservableCollection<ProjectModel> Projects { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc, dùng để bind lên View.</summary>
        public ObservableCollection<ProjectModel> FilteredProjects { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand CreateProjectCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }

        public ProjectVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            CreateProjectCommand = new RelayCommand(_ => CreateProject());
            OpenDetailCommand = new RelayCommand(p => SelectedProject = p as ProjectModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedProject = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is ProjectStatus s ? s : (ProjectStatus?)null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/projects thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Projects.Clear();

            Projects.Add(new ProjectModel
            {
                Name = "Hệ thống ERP nội bộ",
                Description = "Xây dựng phân hệ quản lý nhân sự và chấm công cho toàn công ty.",
                Status = ProjectStatus.InProgress,
                Kind = ProjectKind.Internal,
                StartDate = DateTime.Now.AddMonths(-2),
                Progress = 62,
                TaskTotal = 48,
                TaskDone = 30,
                Members =
                {
                    new ProjectMemberModel { FullName = "Trần Văn Bình", Role = ProjectRole.Owner },
                    new ProjectMemberModel { FullName = "Lê Thị Hoa", Role = ProjectRole.TechnicalLeader },
                    new ProjectMemberModel { FullName = "Nguyễn Văn A", Role = ProjectRole.Developer },
                }
            });

            Projects.Add(new ProjectModel
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
                Members =
                {
                    new ProjectMemberModel { FullName = "Phạm Minh Tuấn", Role = ProjectRole.ProjectManager },
                    new ProjectMemberModel { FullName = "Đỗ Thu Trang", Role = ProjectRole.QaQc },
                }
            });

            Projects.Add(new ProjectModel
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
                Members =
                {
                    new ProjectMemberModel { FullName = "Ngô Quốc Huy", Role = ProjectRole.Owner },
                }
            });

            Projects.Add(new ProjectModel
            {
                Name = "Nền tảng học trực tuyến",
                Description = "Tạm dừng do chờ ký lại hợp đồng với cơ sở đào tạo.",
                Status = ProjectStatus.Paused,
                Kind = ProjectKind.Internal,
                StartDate = DateTime.Now.AddMonths(-3),
                Progress = 45,
                TaskTotal = 40,
                TaskDone = 18,
                Members =
                {
                    new ProjectMemberModel { FullName = "Vũ Thị Mai", Role = ProjectRole.ProjectManager },
                    new ProjectMemberModel { FullName = "Đặng Văn Long", Role = ProjectRole.Intern },
                }
            });

            ApplyFilter();

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
            foreach (var p in query)
                FilteredProjects.Add(p);
        }

        /// <summary>Mở panel "Tạo dự án mới" (overlay), tạo AddProjectVM mới mỗi lần mở
        /// và gán callback để nhận ProjectModel vừa tạo hoặc đóng panel khi huỷ.</summary>
        private void CreateProject()
        {
            // Đóng panel chi tiết nếu đang mở, tránh chồng 2 overlay cùng lúc
            SelectedProject = null;

            var vm = new AddProjectVM();

            vm.OnSaved = project =>
            {
                // TODO: khi có service thật, có thể gọi lại LoadAsync() thay vì chèn trực tiếp vào danh sách cục bộ
                Projects.Insert(0, project);
                ApplyFilter();

                IsAddingProject = false;
                AddProjectVM = null;
            };

            vm.OnCancelled = () =>
            {
                IsAddingProject = false;
                AddProjectVM = null;
            };

            AddProjectVM = vm;
            IsAddingProject = true;
        }
    }
}