using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    /// <summary>
    /// ViewModel cho màn "Mã nguồn": chọn dự án → chọn môi trường (Dev/Test/Production, mỗi môi trường
    /// có nơi lưu cục bộ riêng) → nếu chưa cấu hình/clone thì yêu cầu chọn thư mục cục bộ → hiển thị cây
    /// kiến trúc dự án (component tree) + xem/chỉnh sửa mã nguồn + tạo báo lỗi gắn với file/dòng cụ thể.
    /// </summary>
    public class SourceVM : ViewModelBase
    {
        // TODO: thay bằng tên nhân sự đang đăng nhập lấy từ phiên làm việc thực tế.
        private const string CurrentUserName = "Lê Thị Hoa";

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ObservableCollection<SourceProjectOption> ProjectOptions { get; } = new();

        private SourceProjectOption _selectedProject;
        public SourceProjectOption SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedProject));

                SelectedFile = null;
                CodeContent = string.Empty;
                IsEditingCode = false;

                RefreshEnvironmentView();
            }
        }
        public bool HasSelectedProject => SelectedProject != null;

        private SourceEnvironment _currentEnvironment = SourceEnvironment.Development;
        /// <summary>Thẻ môi trường đang xem: Dev / Test / Production — mỗi môi trường có nơi lưu cục
        /// bộ, cây kiến trúc và danh sách báo lỗi độc lập.</summary>
        public SourceEnvironment CurrentEnvironment
        {
            get => _currentEnvironment;
            set
            {
                _currentEnvironment = value;
                OnPropertyChanged();

                SelectedFile = null;
                CodeContent = string.Empty;
                IsEditingCode = false;

                RefreshEnvironmentView();
            }
        }

        private SourceEnvironmentData CurrentEnvironmentData =>
            SelectedProject != null && SelectedProject.EnvironmentData.TryGetValue(CurrentEnvironment, out var data)
                ? data
                : null;

        /// <summary>True nếu môi trường đang chọn đã được cấu hình/clone mã nguồn về máy — quyết định
        /// hiển thị panel "chọn nơi lưu cục bộ" hay panel cây + mã nguồn.</summary>
        public bool HasLocalPath => CurrentEnvironmentData?.HasLocalPath ?? false;

        /// <summary>Đường dẫn nhập/chọn tạm thời khi cấu hình nơi lưu cục bộ cho môi trường hiện tại.</summary>
        private string _localPathInput;
        public string LocalPathInput { get => _localPathInput; set { _localPathInput = value; OnPropertyChanged(); } }

        private string _localPathError;
        public string LocalPathError { get => _localPathError; set { _localPathError = value; OnPropertyChanged(); } }

        /// <summary>Cây kiến trúc dự án (component tree) của môi trường đang chọn.</summary>
        public ObservableCollection<SourceFileNode> CurrentFileTree { get; } = new();

        private SourceFileNode _selectedFile;
        public SourceFileNode SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedFile));

                if (_selectedFile != null && !_selectedFile.IsFolder)
                {
                    CodeContent = _selectedFile.Content;
                    IsEditingCode = false;
                }
            }
        }
        public bool HasSelectedFile => SelectedFile != null && !SelectedFile.IsFolder;

        private string _codeContent;
        public string CodeContent { get => _codeContent; set { _codeContent = value; OnPropertyChanged(); } }

        private bool _isEditingCode;
        public bool IsEditingCode { get => _isEditingCode; set { _isEditingCode = value; OnPropertyChanged(); } }

        /// <summary>Danh sách báo lỗi mã nguồn của môi trường đang chọn.</summary>
        public ObservableCollection<SourceIssueModel> CurrentIssues { get; } = new();

        // ===== Panel "Tạo báo lỗi mã nguồn" =====
        private bool _isReportingIssue;
        public bool IsReportingIssue { get => _isReportingIssue; set { _isReportingIssue = value; OnPropertyChanged(); } }

        private string _issueTitle;
        public string IssueTitle { get => _issueTitle; set { _issueTitle = value; OnPropertyChanged(); } }

        private string _issueDescription;
        public string IssueDescription { get => _issueDescription; set { _issueDescription = value; OnPropertyChanged(); } }

        private IssueSeverity _issueSeverityInput = IssueSeverity.Medium;
        public IssueSeverity IssueSeverityInput { get => _issueSeverityInput; set { _issueSeverityInput = value; OnPropertyChanged(); } }

        private int? _issueLineNumber;
        public int? IssueLineNumber { get => _issueLineNumber; set { _issueLineNumber = value; OnPropertyChanged(); } }

        private string _issueError;
        public string IssueError { get => _issueError; set { _issueError = value; OnPropertyChanged(); } }

        /// <summary>Gán từ code-behind (SourceView) để mở hộp thoại chọn thư mục cục bộ — VM không phụ
        /// thuộc trực tiếp vào API hộp thoại của WPF để giữ đúng vai trò MVVM.</summary>
        public Func<string> RequestFolderPath { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand SetEnvironmentCommand { get; }
        public ICommand BrowseLocalPathCommand { get; }
        public ICommand ConfirmLocalPathCommand { get; }
        public ICommand SelectFileCommand { get; }
        public ICommand EditCodeCommand { get; }
        public ICommand SaveCodeCommand { get; }
        public ICommand CancelEditCodeCommand { get; }
        public ICommand OpenReportIssueCommand { get; }
        public ICommand SaveIssueCommand { get; }
        public ICommand CancelReportIssueCommand { get; }

        public SourceVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            SetEnvironmentCommand = new RelayCommand(p => CurrentEnvironment = p is SourceEnvironment e ? e : SourceEnvironment.Development);
            BrowseLocalPathCommand = new RelayCommand(_ => BrowseLocalPath());
            ConfirmLocalPathCommand = new RelayCommand(async _ => await ConfirmLocalPathAsync());
            SelectFileCommand = new RelayCommand(p => SelectedFile = p as SourceFileNode);
            EditCodeCommand = new RelayCommand(_ => IsEditingCode = true, _ => HasSelectedFile);
            SaveCodeCommand = new RelayCommand(async _ => await SaveCodeAsync());
            CancelEditCodeCommand = new RelayCommand(_ => CancelEditCode());
            OpenReportIssueCommand = new RelayCommand(_ => OpenReportIssue(), _ => HasSelectedFile);
            SaveIssueCommand = new RelayCommand(async _ => await SaveIssueAsync());
            CancelReportIssueCommand = new RelayCommand(_ => IsReportingIssue = false);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/projects (dự án nhân sự tham gia) + GET /projects/{id}/source-config
            // (nơi lưu cục bộ đã lưu theo từng môi trường + cây kiến trúc + issues) thay cho dữ liệu mẫu.
            await Task.Delay(300);

            ProjectOptions.Clear();

            var p1 = new SourceProjectOption
            {
                ProjectName = "Hệ thống ERP nội bộ",
                RepositoryUrl = "https://git.taskmind.vn/erp-noibo.git"
            };
            p1.EnvironmentData[SourceEnvironment.Development].LocalPath = @"C:\Sources\erp-noibo\dev";
            p1.EnvironmentData[SourceEnvironment.Development].RootNodes.Add(BuildSampleTree());
            SeedIssue(p1.EnvironmentData[SourceEnvironment.Development], SourceEnvironment.Development);
            ProjectOptions.Add(p1);

            var p2 = new SourceProjectOption
            {
                ProjectName = "Website thương mại điện tử ABC",
                RepositoryUrl = "https://git.taskmind.vn/ecommerce-abc.git"
            };
            ProjectOptions.Add(p2);

            SelectedProject = ProjectOptions.FirstOrDefault();

            IsBusy = false;
        }

        private SourceFileNode BuildSampleTree()
        {
            var root = new SourceFileNode { Name = "src", IsFolder = true, FullPath = "src" };

            var models = new SourceFileNode { Name = "Models", IsFolder = true, FullPath = "src/Models" };
            models.Children.Add(new SourceFileNode
            {
                Name = "AttendanceModel.cs",
                FullPath = "src/Models/AttendanceModel.cs",
                Content = "namespace Erp.Models\n{\n    public class AttendanceModel\n    {\n        public Guid Id { get; set; }\n        public DateTime CheckIn { get; set; }\n        public DateTime? CheckOut { get; set; }\n    }\n}"
            });

            var services = new SourceFileNode { Name = "Services", IsFolder = true, FullPath = "src/Services" };
            services.Children.Add(new SourceFileNode
            {
                Name = "AttendanceService.cs",
                FullPath = "src/Services/AttendanceService.cs",
                Content = "namespace Erp.Services\n{\n    public class AttendanceService\n    {\n        public void CheckIn(Guid staffId)\n        {\n            // TODO: kiểm tra nhân sự đã check-in trong ngày hay chưa trước khi ghi nhận\n        }\n    }\n}"
            });

            root.Children.Add(models);
            root.Children.Add(services);
            root.Children.Add(new SourceFileNode
            {
                Name = "README.md",
                FullPath = "README.md",
                Content = "# ERP nội bộ\nModule chấm công."
            });

            return root;
        }

        private void SeedIssue(SourceEnvironmentData data, SourceEnvironment env)
        {
            data.Issues.Add(new SourceIssueModel
            {
                FilePath = "src/Services/AttendanceService.cs",
                LineNumber = 8,
                Title = "Chưa xử lý check-in trùng trong ngày",
                Description = "Hàm CheckIn chưa kiểm tra nhân sự đã check-in trong ngày hay chưa, có thể tạo bản ghi trùng.",
                Severity = IssueSeverity.High,
                Status = IssueStatus.Open,
                CreatedBy = CurrentUserName,
                Environment = env
            });
        }

        /// <summary>Nạp lại cây kiến trúc + issues theo dự án/môi trường đang chọn lên các collection bind UI.</summary>
        private void RefreshEnvironmentView()
        {
            CurrentFileTree.Clear();
            CurrentIssues.Clear();
            LocalPathError = string.Empty;

            var data = CurrentEnvironmentData;
            OnPropertyChanged(nameof(HasLocalPath));

            if (data == null)
            {
                LocalPathInput = string.Empty;
                return;
            }

            LocalPathInput = data.LocalPath;

            foreach (var node in data.RootNodes)
                CurrentFileTree.Add(node);

            foreach (var issue in data.Issues)
                CurrentIssues.Add(issue);
        }

        private void BrowseLocalPath()
        {
            var path = RequestFolderPath?.Invoke();
            if (!string.IsNullOrWhiteSpace(path))
                LocalPathInput = path;
        }

        /// <summary>Lưu nơi lưu cục bộ cho môi trường hiện tại — nếu là lần đầu cấu hình, backend cần
        /// thực hiện clone repo (RepositoryUrl) vào đúng thư mục này.</summary>
        private async Task ConfirmLocalPathAsync()
        {
            LocalPathError = string.Empty;

            if (string.IsNullOrWhiteSpace(LocalPathInput))
            {
                LocalPathError = "Vui lòng chọn hoặc nhập nơi lưu mã nguồn cục bộ.";
                return;
            }

            var data = CurrentEnvironmentData;
            if (data == null) return;

            IsBusy = true;

            // TODO: gọi service PUT /projects/{id}/source-config (theo môi trường) để lưu đường dẫn cục
            // bộ, đồng thời thực hiện "git clone {RepositoryUrl} {LocalPathInput}" nếu thư mục đang trống.
            await Task.Delay(300);

            data.LocalPath = LocalPathInput.Trim();
            data.RootNodes.Clear();
            data.RootNodes.Add(BuildSampleTree());

            RefreshEnvironmentView();

            IsBusy = false;
        }

        private async Task SaveCodeAsync()
        {
            if (SelectedFile == null) return;

            IsBusy = true;

            // TODO: gọi service PUT /projects/{id}/source-files (theo môi trường + FullPath) để ghi nội
            // dung file xuống ổ đĩa/kho mã nguồn thay cho việc cập nhật trực tiếp đối tượng cục bộ.
            await Task.Delay(300);

            SelectedFile.Content = CodeContent;
            IsEditingCode = false;

            IsBusy = false;
        }

        private void CancelEditCode()
        {
            CodeContent = SelectedFile?.Content;
            IsEditingCode = false;
        }

        private void OpenReportIssue()
        {
            if (!HasSelectedFile) return;

            IssueTitle = string.Empty;
            IssueDescription = string.Empty;
            IssueSeverityInput = IssueSeverity.Medium;
            IssueLineNumber = null;
            IssueError = string.Empty;

            IsReportingIssue = true;
        }

        private async Task SaveIssueAsync()
        {
            IssueError = string.Empty;

            if (string.IsNullOrWhiteSpace(IssueTitle))
            {
                IssueError = "Vui lòng nhập tiêu đề lỗi.";
                return;
            }

            if (SelectedFile == null || CurrentEnvironmentData == null) return;

            IsBusy = true;

            // TODO: gọi service POST /projects/{id}/source-issues thay cho việc thêm trực tiếp vào danh
            // sách cục bộ.
            await Task.Delay(300);

            var issue = new SourceIssueModel
            {
                FilePath = SelectedFile.FullPath,
                LineNumber = IssueLineNumber,
                Title = IssueTitle.Trim(),
                Description = IssueDescription?.Trim(),
                Severity = IssueSeverityInput,
                Status = IssueStatus.Open,
                CreatedBy = CurrentUserName,
                Environment = CurrentEnvironment
            };

            CurrentEnvironmentData.Issues.Add(issue);
            CurrentIssues.Add(issue);

            IsBusy = false;
            IsReportingIssue = false;
        }
    }
}