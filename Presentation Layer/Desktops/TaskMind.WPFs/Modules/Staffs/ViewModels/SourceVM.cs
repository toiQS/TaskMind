using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    /// <summary>
    /// ViewModel màn hình "Mã nguồn". Luồng sử dụng: chọn dự án -> nếu chưa clone mã nguồn về máy thì
    /// chọn nơi lưu cục bộ -> xem cây kiến trúc + mã nguồn theo 3 thẻ môi trường (Dev/Test/Product) ->
    /// chỉnh sửa & lưu mã nguồn -> release sang môi trường kế tiếp (chỉ Owner/Technical leader/Project
    /// manager) -> tạo thông báo lỗi trong mã nguồn cần sửa.
    /// </summary>
    public class SourceVM : ViewModelBase
    {
        // TODO: thay bằng tên nhân sự đang đăng nhập lấy từ phiên làm việc thực tế.
        private const string CurrentUserName = "Lê Thị Hoa";

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        // ===== Chọn dự án =====
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
                OnPropertyChanged(nameof(NeedsLocalPath));
                OnPropertyChanged(nameof(CanShowWorkspace));

                LocalPathInput = value?.LocalPath;
                ErrorMessage = string.Empty;

                if (value != null && value.IsCloned)
                    _ = LoadWorkspacesAsync();
            }
        }
        public bool HasSelectedProject => SelectedProject != null;

        /// <summary>True khi đã chọn dự án nhưng chưa clone mã nguồn về máy — cần chọn nơi lưu cục bộ.</summary>
        public bool NeedsLocalPath => SelectedProject != null && !SelectedProject.IsCloned;

        /// <summary>True khi đã sẵn sàng hiển thị khu vực làm việc (đã chọn dự án + đã có mã nguồn cục bộ).</summary>
        public bool CanShowWorkspace => SelectedProject != null && SelectedProject.IsCloned;

        private string _localPathInput;
        public string LocalPathInput { get => _localPathInput; set { _localPathInput = value; OnPropertyChanged(); } }

        // ===== 3 workspace môi trường: Dev / Test / Product =====
        public SourceEnvironmentWorkspace DevWorkspace { get; } = new() { Environment = SourceEnvironment.Dev };
        public SourceEnvironmentWorkspace TestWorkspace { get; } = new() { Environment = SourceEnvironment.Test };
        public SourceEnvironmentWorkspace ProductWorkspace { get; } = new() { Environment = SourceEnvironment.Product };

        private SourceEnvironment _currentEnvironment = SourceEnvironment.Dev;
        public SourceEnvironment CurrentEnvironment
        {
            get => _currentEnvironment;
            set { _currentEnvironment = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentWorkspace)); }
        }

        /// <summary>Workspace đang hiển thị trên UI, tương ứng thẻ (tab) môi trường đang chọn.</summary>
        public SourceEnvironmentWorkspace CurrentWorkspace => CurrentEnvironment switch
        {
            SourceEnvironment.Dev => DevWorkspace,
            SourceEnvironment.Test => TestWorkspace,
            _ => ProductWorkspace
        };

        // ===== Form thêm thông báo lỗi mã nguồn =====
        private bool _isAddingIssue;
        public bool IsAddingIssue { get => _isAddingIssue; set { _isAddingIssue = value; OnPropertyChanged(); } }

        private string _issueTitleInput;
        public string IssueTitleInput { get => _issueTitleInput; set { _issueTitleInput = value; OnPropertyChanged(); } }

        private string _issueDescriptionInput;
        public string IssueDescriptionInput { get => _issueDescriptionInput; set { _issueDescriptionInput = value; OnPropertyChanged(); } }

        private CodeIssueSeverity _issueSeverityInput = CodeIssueSeverity.Medium;
        public CodeIssueSeverity IssueSeverityInput { get => _issueSeverityInput; set { _issueSeverityInput = value; OnPropertyChanged(); } }

        public ICommand BrowseLocalPathCommand { get; }
        public ICommand ConfirmCloneCommand { get; }
        public ICommand SetEnvironmentCommand { get; }
        public ICommand SelectNodeCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand DiscardChangesCommand { get; }
        public ICommand ReleaseCommand { get; }
        public ICommand OpenAddIssueCommand { get; }
        public ICommand CancelAddIssueCommand { get; }
        public ICommand AddIssueCommand { get; }
        public ICommand ResolveIssueCommand { get; }

        public SourceVM()
        {
            BrowseLocalPathCommand = new RelayCommand(_ => BrowseLocalPath());
            ConfirmCloneCommand = new RelayCommand(async _ => await ConfirmCloneAsync(), _ => !string.IsNullOrWhiteSpace(LocalPathInput));
            SetEnvironmentCommand = new RelayCommand(p => CurrentEnvironment = p is SourceEnvironment e ? e : SourceEnvironment.Dev);
            SelectNodeCommand = new RelayCommand(p => SelectNode(p as SourceNodeModel));
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => CurrentWorkspace.IsDirty);
            DiscardChangesCommand = new RelayCommand(_ => DiscardChanges(), _ => CurrentWorkspace.IsDirty);
            ReleaseCommand = new RelayCommand(_ => Release(), _ => CanRelease());
            OpenAddIssueCommand = new RelayCommand(_ => OpenAddIssue());
            CancelAddIssueCommand = new RelayCommand(_ => IsAddingIssue = false);
            AddIssueCommand = new RelayCommand(_ => AddIssue());
            ResolveIssueCommand = new RelayCommand(p => ResolveIssue(p as CodeIssueModel));

            _ = LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/projects (dự án nhân sự tham gia) thay cho dữ liệu mẫu bên dưới;
            // LocalPath nên lấy từ cấu hình máy trạm đã lưu trước đó (nếu nhân sự đã từng clone).
            await Task.Delay(300);

            ProjectOptions.Clear();
            ProjectOptions.Add(new SourceProjectOption
            {
                ProjectName = "Hệ thống ERP nội bộ",
                MyRole = ProjectRole.TechnicalLeader,
                LocalPath = @"C:\Sources\erp-noi-bo"
            });
            ProjectOptions.Add(new SourceProjectOption
            {
                ProjectName = "Website thương mại điện tử ABC",
                MyRole = ProjectRole.Developer,
                LocalPath = null // chưa clone về máy -> màn hình sẽ yêu cầu chọn nơi lưu cục bộ
            });

            IsBusy = false;

            SelectedProject = ProjectOptions.FirstOrDefault();
        }

        /// <summary>Mở hộp thoại chọn thư mục hệ điều hành để làm nơi lưu mã nguồn cục bộ.</summary>
        private void BrowseLocalPath()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Chọn thư mục lưu mã nguồn cục bộ"
            };

            if (dialog.ShowDialog() == true)
                LocalPathInput = dialog.FolderName;
        }

        private async Task ConfirmCloneAsync()
        {
            if (SelectedProject == null) return;

            if (string.IsNullOrWhiteSpace(LocalPathInput))
            {
                ErrorMessage = "Vui lòng chọn thư mục lưu mã nguồn cục bộ.";
                return;
            }

            ErrorMessage = string.Empty;
            IsBusy = true;

            // TODO: gọi service/git clone thật vào LocalPathInput thay cho việc chỉ gán đường dẫn.
            await Task.Delay(400);

            SelectedProject.LocalPath = LocalPathInput;
            OnPropertyChanged(nameof(NeedsLocalPath));
            OnPropertyChanged(nameof(CanShowWorkspace));

            await LoadWorkspacesAsync();

            IsBusy = false;
        }

        private async Task LoadWorkspacesAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /projects/{id}/source?env=dev|test|product (cây kiến trúc + nội dung
            // file, đọc trực tiếp từ SelectedProject.LocalPath) thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            DevWorkspace.RootNodes.Clear();
            foreach (var n in BuildSampleTree(includeUnreleasedFile: true))
                DevWorkspace.RootNodes.Add(n);
            DevWorkspace.SetSelectedFile(null);
            DevWorkspace.Issues.Clear();
            DevWorkspace.Issues.Add(new CodeIssueModel
            {
                Title = "NullReferenceException khi Profile chưa tải xong",
                Description = "Cần kiểm tra null trước khi truy cập Profile.Skills nếu vào màn hình quá nhanh.",
                FileName = "ProfileVM.cs",
                LineNumber = 128,
                Severity = CodeIssueSeverity.High,
                Status = CodeIssueStatus.Open,
                CreatedBy = "Trần Văn Bình",
                CreatedDate = DateTime.Now.AddDays(-1)
            });

            TestWorkspace.RootNodes.Clear();
            foreach (var n in BuildSampleTree(includeUnreleasedFile: false))
                TestWorkspace.RootNodes.Add(n);
            TestWorkspace.SetSelectedFile(null);
            TestWorkspace.ReleaseLogs.Clear();
            TestWorkspace.ReleaseLogs.Add(new ReleaseLogModel
            {
                FromEnvironment = SourceEnvironment.Dev,
                ToEnvironment = SourceEnvironment.Test,
                ReleasedBy = "Trần Văn Bình",
                ReleasedDate = DateTime.Now.AddDays(-3),
                Note = "Release sprint 4."
            });

            ProductWorkspace.RootNodes.Clear();
            foreach (var n in BuildSampleTree(includeUnreleasedFile: false, stable: true))
                ProductWorkspace.RootNodes.Add(n);
            ProductWorkspace.SetSelectedFile(null);
            ProductWorkspace.ReleaseLogs.Clear();
            ProductWorkspace.ReleaseLogs.Add(new ReleaseLogModel
            {
                FromEnvironment = SourceEnvironment.Test,
                ToEnvironment = SourceEnvironment.Product,
                ReleasedBy = "Trần Văn Bình",
                ReleasedDate = DateTime.Now.AddDays(-10),
                Note = "Release chính thức phiên bản 1.2."
            });

            CurrentEnvironment = SourceEnvironment.Dev;

            IsBusy = false;
        }

        private static IEnumerable<SourceNodeModel> BuildSampleTree(bool includeUnreleasedFile, bool stable = false)
        {
            var models = new SourceNodeModel { Name = "Models", Type = SourceNodeType.Folder };
            models.Children.Add(new SourceNodeModel
            {
                Name = "ProfileModel.cs",
                Type = SourceNodeType.File,
                RelativePath = "Models/ProfileModel.cs",
                Content = "public class ProfileModel\n{\n    public string FullName { get; set; }\n    public List<SkillItemModel> Skills { get; set; } = new();\n}"
            });

            var viewModels = new SourceNodeModel { Name = "ViewModels", Type = SourceNodeType.Folder };
            viewModels.Children.Add(new SourceNodeModel
            {
                Name = "ProfileVM.cs",
                Type = SourceNodeType.File,
                RelativePath = "ViewModels/ProfileVM.cs",
                Content = stable
                    ? "public class ProfileVM : ViewModelBase\n{\n    // Bản ổn định đã release Product.\n}"
                    : "public class ProfileVM : ViewModelBase\n{\n    public ProfileModel Profile { get; set; }\n\n    // TODO: kiểm tra null trước khi truy cập Profile.Skills\n}"
            });

            if (includeUnreleasedFile)
            {
                viewModels.Children.Add(new SourceNodeModel
                {
                    Name = "SourceVM.cs",
                    Type = SourceNodeType.File,
                    RelativePath = "ViewModels/SourceVM.cs",
                    Content = "// Đang phát triển tại Dev, chưa release sang Test."
                });
            }

            var views = new SourceNodeModel { Name = "Views", Type = SourceNodeType.Folder };
            views.Children.Add(new SourceNodeModel
            {
                Name = "ProfileView.xaml",
                Type = SourceNodeType.File,
                RelativePath = "Views/ProfileView.xaml",
                Content = "<UserControl x:Class=\"...ProfileView\">\n    <!-- ... -->\n</UserControl>"
            });

            return new[] { models, viewModels, views };
        }

        private void SelectNode(SourceNodeModel node)
        {
            if (node == null) return;

            if (node.IsFolder)
            {
                node.IsExpanded = !node.IsExpanded;
                return;
            }

            if (CurrentWorkspace.IsDirty)
            {
                ErrorMessage = "Bạn có thay đổi chưa lưu ở file hiện tại. Hãy Lưu hoặc Huỷ thay đổi trước khi mở file khác.";
                return;
            }

            ErrorMessage = string.Empty;
            CurrentWorkspace.SetSelectedFile(node);
        }

        private void SaveFile()
        {
            var ws = CurrentWorkspace;
            if (ws.SelectedFile == null || !ws.IsDirty) return;

            // TODO: gọi service PUT /projects/{id}/source/{env}/{path} để lưu nội dung file thật.
            ws.SelectedFile.Content = ws.EditableContent;
            ws.IsDirty = false;
        }

        private void DiscardChanges()
        {
            var ws = CurrentWorkspace;
            if (ws.SelectedFile == null) return;

            ws.EditableContent = ws.SelectedFile.Content;
            ws.IsDirty = false;
        }

        private bool CanRelease()
            => SelectedProject != null
               && SelectedProject.CanRelease
               && CurrentEnvironment != SourceEnvironment.Product
               && !CurrentWorkspace.IsDirty;

        /// <summary>Release toàn bộ cây mã nguồn từ môi trường hiện tại sang môi trường kế tiếp
        /// (Dev -> Test hoặc Test -> Product) — chỉ Owner/Technical leader/Project manager (mục 3).</summary>
        private void Release()
        {
            if (!CanRelease()) return;

            var target = CurrentEnvironment == SourceEnvironment.Dev ? TestWorkspace : ProductWorkspace;

            // TODO: gọi service POST /projects/{id}/source/release { from, to } thay cho việc sao chép cục bộ.
            target.RootNodes.Clear();
            foreach (var node in CurrentWorkspace.RootNodes)
                target.RootNodes.Add(node.DeepClone());

            target.SetSelectedFile(null);
            target.ReleaseLogs.Insert(0, new ReleaseLogModel
            {
                FromEnvironment = CurrentEnvironment,
                ToEnvironment = target.Environment,
                ReleasedBy = CurrentUserName,
                ReleasedDate = DateTime.Now
            });

            CurrentEnvironment = target.Environment;
        }

        private void OpenAddIssue()
        {
            IssueTitleInput = string.Empty;
            IssueDescriptionInput = string.Empty;
            IssueSeverityInput = CodeIssueSeverity.Medium;
            ErrorMessage = string.Empty;
            IsAddingIssue = true;
        }

        private void AddIssue()
        {
            if (string.IsNullOrWhiteSpace(IssueTitleInput))
            {
                ErrorMessage = "Vui lòng nhập tiêu đề thông báo lỗi.";
                return;
            }

            // TODO: gọi service POST /projects/{id}/source/{env}/issues thay cho việc thêm cục bộ.
            CurrentWorkspace.Issues.Insert(0, new CodeIssueModel
            {
                Title = IssueTitleInput.Trim(),
                Description = IssueDescriptionInput?.Trim(),
                FileName = CurrentWorkspace.SelectedFile?.Name ?? "(chưa chọn file)",
                Severity = IssueSeverityInput,
                Status = CodeIssueStatus.Open,
                CreatedBy = CurrentUserName
            });
            CurrentWorkspace.TouchIssues();

            IsAddingIssue = false;
        }

        private void ResolveIssue(CodeIssueModel issue)
        {
            if (issue == null || issue.Status == CodeIssueStatus.Resolved) return;

            // TODO: gọi service PATCH /projects/{id}/source/issues/{issueId}/resolve
            issue.Status = CodeIssueStatus.Resolved;

            CurrentWorkspace.TouchIssues();
        }
    }
}