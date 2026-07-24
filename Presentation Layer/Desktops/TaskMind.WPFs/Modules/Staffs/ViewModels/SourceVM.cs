using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    /// <summary>
    /// ViewModel màn hình "Mã nguồn" — sidebar 4 tab kiểu GitHub Desktop (Duyệt/Thay đổi/Lịch sử/Lỗi).
    /// Luồng: chọn dự án -> clone nếu cần -> tab "Duyệt" xem cây + sửa nhiều file song song (mỗi file
    /// giữ bản nháp riêng) -> tab "Thay đổi" gom các file đang sửa, nhập message rồi Commit -> tab
    /// "Lịch sử" xem lại từng commit dạng diff (+/- theo dòng), có thể Revert -> Release Dev->Test->
    /// Product (chỉ Owner/Technical leader/Project manager), mỗi lần release cũng được ghi vào Lịch sử.
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
        public bool NeedsLocalPath => SelectedProject != null && !SelectedProject.IsCloned;
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

        public SourceEnvironmentWorkspace CurrentWorkspace => CurrentEnvironment switch
        {
            SourceEnvironment.Dev => DevWorkspace,
            SourceEnvironment.Test => TestWorkspace,
            _ => ProductWorkspace
        };

        // ===== Tab sidebar bên trái (Duyệt / Thay đổi / Lịch sử / Lỗi) =====
        private SourceLeftTab _currentLeftTab = SourceLeftTab.Browse;
        public SourceLeftTab CurrentLeftTab
        {
            get => _currentLeftTab;
            set { _currentLeftTab = value; OnPropertyChanged(); }
        }

        // ===== Form commit (tab "Thay đổi") =====
        private string _commitMessageInput;
        public string CommitMessageInput { get => _commitMessageInput; set { _commitMessageInput = value; OnPropertyChanged(); } }

        private string _commitDescriptionInput;
        public string CommitDescriptionInput { get => _commitDescriptionInput; set { _commitDescriptionInput = value; OnPropertyChanged(); } }

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
        public ICommand SetLeftTabCommand { get; }
        public ICommand SelectNodeCommand { get; }
        public ICommand DiscardDraftCommand { get; }
        public ICommand DiscardAllCommand { get; }
        public ICommand CommitChangesCommand { get; }
        public ICommand SelectCommitCommand { get; }
        public ICommand CloseCommitDetailCommand { get; }
        public ICommand SelectDiffFileCommand { get; }
        public ICommand RevertCommitCommand { get; }
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
            SetLeftTabCommand = new RelayCommand(p => CurrentLeftTab = p is SourceLeftTab t ? t : SourceLeftTab.Browse);
            SelectNodeCommand = new RelayCommand(p => SelectNode(p as SourceNodeModel));
            DiscardDraftCommand = new RelayCommand(p => DiscardDraft(p as SourceNodeModel));
            DiscardAllCommand = new RelayCommand(_ => DiscardAllChanges(), _ => CurrentWorkspace.DirtyFiles.Count > 0);
            CommitChangesCommand = new RelayCommand(_ => CommitChanges(), _ => CanCommit());
            SelectCommitCommand = new RelayCommand(p => SelectCommit(p as SourceCommitModel));
            CloseCommitDetailCommand = new RelayCommand(_ => CurrentWorkspace.SetSelectedCommit(null));
            SelectDiffFileCommand = new RelayCommand(p => { if (CurrentWorkspace.SelectedCommit != null) CurrentWorkspace.SelectedCommit.SelectedFile = p as DiffFileModel; });
            RevertCommitCommand = new RelayCommand(p => RevertCommit(p as SourceCommitModel));
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

            // TODO: gọi service GET /me/projects thay cho dữ liệu mẫu bên dưới.
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
                LocalPath = null
            });

            IsBusy = false;

            SelectedProject = ProjectOptions.FirstOrDefault();
        }

        private void BrowseLocalPath()
        {
            var dialog = new OpenFolderDialog { Title = "Chọn thư mục lưu mã nguồn cục bộ" };
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

            // TODO: gọi service GET /projects/{id}/source?env=... (cây kiến trúc, commit log thật)
            // thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            var oldContent = "public class ProfileVM : ViewModelBase\n{\n    public ProfileModel Profile { get; set; }\n\n    // TODO: kiểm tra null trước khi truy cập Profile.Skills\n}";
            var newContent = "public class ProfileVM : ViewModelBase\n{\n    public ProfileModel Profile { get; set; }\n\n    public bool IsLoaded => Profile != null;\n\n    // Đã kiểm tra null trước khi truy cập Profile.Skills.\n}";

            DevWorkspace.RootNodes.Clear();
            foreach (var n in BuildSampleTree(includeUnreleasedFile: true, profileVmContent: newContent))
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

            // ===== Dữ liệu mẫu: lịch sử commit (kiểu GitHub Desktop) =====
            DevWorkspace.Commits.Clear();

            var diff1 = new DiffFileModel { RelativePath = "ViewModels/ProfileVM.cs", ChangeKind = DiffChangeKind.Modified, OldContent = oldContent, NewContent = newContent };
            var (lines1, added1, removed1) = DiffHelper.Compute(diff1.OldContent, diff1.NewContent);
            diff1.Lines = new ObservableCollection<DiffLineModel>(lines1);
            diff1.AddedCount = added1;
            diff1.RemovedCount = removed1;

            DevWorkspace.Commits.Add(new SourceCommitModel
            {
                Message = "Kiểm tra null trước khi truy cập Skills",
                Description = "Sửa lỗi NullReferenceException khi vào màn hình Profile quá nhanh, thêm IsLoaded để View kiểm tra trạng thái tải.",
                AuthorName = "Trần Văn Bình",
                CommittedDate = DateTime.Now.AddHours(-2),
                Kind = SourceCommitKind.Edit,
                ChangedFiles = new ObservableCollection<DiffFileModel> { diff1 }
            });

            DevWorkspace.Commits.Add(new SourceCommitModel
            {
                Message = "upload files seconds",
                AuthorName = CurrentUserName,
                CommittedDate = DateTime.Now.AddHours(-15),
                Kind = SourceCommitKind.Edit
            });

            DevWorkspace.RefreshDirtyFiles();

            TestWorkspace.RootNodes.Clear();
            foreach (var n in BuildSampleTree(includeUnreleasedFile: false, profileVmContent: oldContent))
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
            TestWorkspace.Commits.Clear();
            TestWorkspace.Commits.Add(new SourceCommitModel
            {
                Message = $"Release từ Development lên Testing",
                Description = "Release sprint 4.",
                AuthorName = "Trần Văn Bình",
                CommittedDate = DateTime.Now.AddDays(-3),
                Kind = SourceCommitKind.Release
            });
            TestWorkspace.RefreshDirtyFiles();

            ProductWorkspace.RootNodes.Clear();
            foreach (var n in BuildSampleTree(includeUnreleasedFile: false, stable: true, profileVmContent: oldContent))
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
            ProductWorkspace.Commits.Clear();
            ProductWorkspace.Commits.Add(new SourceCommitModel
            {
                Message = "Release từ Testing lên Production",
                Description = "Release chính thức phiên bản 1.2.",
                AuthorName = "Trần Văn Bình",
                CommittedDate = DateTime.Now.AddDays(-10),
                Kind = SourceCommitKind.Release
            });
            ProductWorkspace.RefreshDirtyFiles();

            CurrentEnvironment = SourceEnvironment.Dev;
            CurrentLeftTab = SourceLeftTab.Browse;

            IsBusy = false;
        }

        private static IEnumerable<SourceNodeModel> BuildSampleTree(bool includeUnreleasedFile, bool stable = false, string profileVmContent = null)
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
                    : profileVmContent
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

        // ===== Duyệt cây kiến trúc + mở file =====
        private void SelectNode(SourceNodeModel node)
        {
            if (node == null) return;

            if (node.IsFolder)
            {
                node.IsExpanded = !node.IsExpanded;
                return;
            }

            ErrorMessage = string.Empty;
            CurrentWorkspace.SetSelectedFile(node);
            CurrentLeftTab = SourceLeftTab.Browse;
        }

        // ===== Tab "Thay đổi" =====
        private void DiscardDraft(SourceNodeModel node)
        {
            if (node == null || !node.IsDirty) return;
            node.DiscardDraft();
            CurrentWorkspace.RefreshDirtyFiles();
        }

        private void DiscardAllChanges()
        {
            foreach (var f in CurrentWorkspace.DirtyFiles.ToList())
                f.DiscardDraft();
            CurrentWorkspace.RefreshDirtyFiles();
        }

        private bool CanCommit() => CurrentWorkspace.DirtyFiles.Count > 0 && !string.IsNullOrWhiteSpace(CommitMessageInput);

        /// <summary>Gộp toàn bộ file đang có thay đổi thành 1 commit — tính diff từng file (dùng để xem
        /// lại/revert sau này), rồi chốt DraftContent thành Content chính thức.</summary>
        private void CommitChanges()
        {
            var dirty = CurrentWorkspace.DirtyFiles.ToList();
            if (dirty.Count == 0) return;

            if (string.IsNullOrWhiteSpace(CommitMessageInput))
            {
                ErrorMessage = "Vui lòng nhập nội dung commit.";
                return;
            }

            var commit = new SourceCommitModel
            {
                Message = CommitMessageInput.Trim(),
                Description = CommitDescriptionInput?.Trim(),
                AuthorName = CurrentUserName,
                Kind = SourceCommitKind.Edit
            };

            foreach (var file in dirty)
            {
                var (lines, added, removed) = DiffHelper.Compute(file.Content, file.DraftContent);

                commit.ChangedFiles.Add(new DiffFileModel
                {
                    RelativePath = file.RelativePath,
                    ChangeKind = DiffChangeKind.Modified,
                    OldContent = file.Content,
                    NewContent = file.DraftContent,
                    AddedCount = added,
                    RemovedCount = removed,
                    Lines = new ObservableCollection<DiffLineModel>(lines)
                });

                // TODO: gọi service PUT /projects/{id}/source/{env}/{path} để lưu nội dung file thật.
                file.CommitDraft();
            }

            CurrentWorkspace.Commits.Insert(0, commit);
            CurrentWorkspace.RefreshDirtyFiles();

            CommitMessageInput = string.Empty;
            CommitDescriptionInput = string.Empty;
            ErrorMessage = string.Empty;
        }

        // ===== Tab "Lịch sử" =====
        private void SelectCommit(SourceCommitModel commit)
        {
            if (commit == null) return;
            CurrentWorkspace.SetSelectedCommit(commit);
        }

        /// <summary>Hoàn tác 1 commit bằng cách tạo commit mới (Kind = Revert) đảo ngược nội dung cũ/mới
        /// của từng file, đúng tinh thần "Revert" trong ảnh GitHub Desktop bạn gửi.</summary>
        private void RevertCommit(SourceCommitModel commit)
        {
            if (commit == null || commit.ChangedFiles.Count == 0) return;

            var revert = new SourceCommitModel
            {
                Message = $"Revert \"{commit.Message}\"",
                Description = $"This reverts commit {commit.Id}.",
                AuthorName = CurrentUserName,
                Kind = SourceCommitKind.Revert,
                RevertedCommitId = commit.Id,
                RevertedCommitMessage = commit.Message
            };

            foreach (var changed in commit.ChangedFiles)
            {
                var node = CurrentWorkspace.FlattenFiles().FirstOrDefault(f => f.RelativePath == changed.RelativePath);
                if (node == null) continue;

                var (lines, added, removed) = DiffHelper.Compute(changed.NewContent, changed.OldContent);

                revert.ChangedFiles.Add(new DiffFileModel
                {
                    RelativePath = changed.RelativePath,
                    ChangeKind = DiffChangeKind.Modified,
                    OldContent = changed.NewContent,
                    NewContent = changed.OldContent,
                    AddedCount = added,
                    RemovedCount = removed,
                    Lines = new ObservableCollection<DiffLineModel>(lines)
                });

                // TODO: gọi service PUT /projects/{id}/source/{env}/{path} để lưu nội dung file thật.
                node.Content = changed.OldContent;
                node.DiscardDraft();
            }

            CurrentWorkspace.Commits.Insert(0, revert);
            CurrentWorkspace.RefreshDirtyFiles();
            CurrentWorkspace.SetSelectedCommit(revert);
        }

        // ===== Release Dev -> Test -> Product =====
        private bool CanRelease()
            => SelectedProject != null
               && SelectedProject.CanRelease
               && CurrentEnvironment != SourceEnvironment.Product;

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
            target.Commits.Insert(0, new SourceCommitModel
            {
                Message = $"Release từ {CurrentEnvironment} lên {target.Environment}",
                AuthorName = CurrentUserName,
                Kind = SourceCommitKind.Release
            });
            target.RefreshDirtyFiles();

            CurrentEnvironment = target.Environment;
            CurrentLeftTab = SourceLeftTab.History;
        }

        // ===== Thông báo lỗi mã nguồn =====
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

            issue.Status = CodeIssueStatus.Resolved;
            CurrentWorkspace.TouchIssues();
        }
    }
}