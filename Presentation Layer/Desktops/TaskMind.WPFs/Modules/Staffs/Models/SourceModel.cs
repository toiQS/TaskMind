using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>
    /// Module "Mã nguồn" — lấy cảm hứng bố cục từ GitHub Desktop: sidebar 2 khu vực chính
    /// "Thay đổi" (working changes, giống working directory) và "Lịch sử" (commit log, có thể
    /// xem diff + revert từng commit), cộng thêm 2 tab tiện ích "Duyệt" (cây kiến trúc + sửa file)
    /// và "Lỗi" (thông báo lỗi mã nguồn). PM/Technical leader/Owner vẫn giữ khả năng release
    /// Dev -> Test -> Product như trước; mỗi lần release cũng được ghi vào lịch sử commit.
    /// </summary>

    public enum SourceEnvironment
    {
        Dev,
        Test,
        Product
    }

    public enum SourceNodeType
    {
        Folder,
        File
    }

    public enum CodeIssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum CodeIssueStatus
    {
        Open,
        InProgress,
        Resolved
    }

    /// <summary>Tab đang chọn ở sidebar bên trái (giống 2 tab Changes/History của GitHub Desktop,
    /// cộng thêm 2 tab tiện ích riêng của TaskMind).</summary>
    public enum SourceLeftTab
    {
        Browse,   // Duyệt cây kiến trúc + sửa file
        Changes,  // Các file đang có thay đổi chưa commit
        History,  // Lịch sử commit (có thể xem diff + revert)
        Issues    // Thông báo lỗi mã nguồn
    }

    // ===================== Diff (dùng chung cho Changes preview & History detail) =====================

    public enum DiffLineType
    {
        Context,
        Added,
        Removed
    }

    public enum DiffChangeKind
    {
        Added,
        Modified,
        Deleted
    }

    /// <summary>Một dòng trong khung diff — có thể chỉ có số dòng cũ (bị xoá), chỉ số dòng mới
    /// (được thêm), hoặc cả hai (không đổi).</summary>
    public class DiffLineModel
    {
        public int? OldLineNumber { get; set; }
        public int? NewLineNumber { get; set; }
        public string Content { get; set; }
        public DiffLineType Type { get; set; } = DiffLineType.Context;
    }

    /// <summary>Diff của MỘT file trong MỘT commit — lưu cả nội dung cũ/mới đầy đủ (dùng để revert
    /// chính xác) lẫn danh sách dòng diff đã tính sẵn (dùng để hiển thị).</summary>
    public class DiffFileModel
    {
        public string RelativePath { get; set; }
        public DiffChangeKind ChangeKind { get; set; } = DiffChangeKind.Modified;

        public string OldContent { get; set; }
        public string NewContent { get; set; }

        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }

        public ObservableCollection<DiffLineModel> Lines { get; set; } = new();

        public string FileName => string.IsNullOrEmpty(RelativePath)
            ? string.Empty
            : RelativePath.Contains('/') ? RelativePath[(RelativePath.LastIndexOf('/') + 1)..] : RelativePath;

        public string StatsDisplay => $"+{AddedCount} -{RemovedCount}";
    }

    /// <summary>Loại commit trong lịch sử: chỉnh sửa thông thường, release sang môi trường kế tiếp,
    /// hoặc hoàn tác (revert) một commit trước đó.</summary>
    public enum SourceCommitKind
    {
        Edit,
        Release,
        Revert
    }

    /// <summary>Một "commit" mã nguồn — gộp nhiều file thay đổi cùng lúc kèm message, giống git log.
    /// Kế thừa ViewModelBase để tô sáng dòng đang chọn trong danh sách Lịch sử.</summary>
    public class SourceCommitModel : ViewModelBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..7];
        public string Message { get; set; }
        public string Description { get; set; }
        public string AuthorName { get; set; }
        public DateTime CommittedDate { get; set; } = DateTime.Now;
        public SourceCommitKind Kind { get; set; } = SourceCommitKind.Edit;

        /// <summary>Chỉ có giá trị khi Kind = Revert.</summary>
        public string RevertedCommitId { get; set; }
        public string RevertedCommitMessage { get; set; }

        public ObservableCollection<DiffFileModel> ChangedFiles { get; set; } = new();

        private DiffFileModel _selectedFile;
        public DiffFileModel SelectedFile
        {
            get => _selectedFile ?? ChangedFiles.FirstOrDefault();
            set { _selectedFile = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        public int TotalAdded => ChangedFiles.Sum(f => f.AddedCount);
        public int TotalRemoved => ChangedFiles.Sum(f => f.RemovedCount);
        public string StatsDisplay => $"+{TotalAdded} -{TotalRemoved}";

        public string AuthorInitial => string.IsNullOrWhiteSpace(AuthorName) ? "?" : AuthorName.Trim()[0].ToString().ToUpper();
        public bool IsRevert => Kind == SourceCommitKind.Revert;
        public bool HasChangedFiles => ChangedFiles.Count > 0;
    }

    /// <summary>Thuật toán diff dòng-với-dòng (LCS) đơn giản — đủ dùng để hiển thị preview trong app,
    /// không thay thế cho một diff engine Git thật.</summary>
    public static class DiffHelper
    {
        public static (List<DiffLineModel> Lines, int Added, int Removed) Compute(string oldText, string newText)
        {
            var oldLines = (oldText ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            var newLines = (newText ?? string.Empty).Replace("\r\n", "\n").Split('\n');

            int n = oldLines.Length, m = newLines.Length;
            var dp = new int[n + 1, m + 1];
            for (int i = n - 1; i >= 0; i--)
                for (int j = m - 1; j >= 0; j--)
                    dp[i, j] = oldLines[i] == newLines[j] ? dp[i + 1, j + 1] + 1 : Math.Max(dp[i + 1, j], dp[i, j + 1]);

            var result = new List<DiffLineModel>();
            int added = 0, removed = 0, a = 0, b = 0;

            while (a < n && b < m)
            {
                if (oldLines[a] == newLines[b])
                {
                    result.Add(new DiffLineModel { Type = DiffLineType.Context, OldLineNumber = a + 1, NewLineNumber = b + 1, Content = oldLines[a] });
                    a++; b++;
                }
                else if (dp[a + 1, b] >= dp[a, b + 1])
                {
                    result.Add(new DiffLineModel { Type = DiffLineType.Removed, OldLineNumber = a + 1, Content = oldLines[a] });
                    removed++; a++;
                }
                else
                {
                    result.Add(new DiffLineModel { Type = DiffLineType.Added, NewLineNumber = b + 1, Content = newLines[b] });
                    added++; b++;
                }
            }
            while (a < n) { result.Add(new DiffLineModel { Type = DiffLineType.Removed, OldLineNumber = a + 1, Content = oldLines[a] }); removed++; a++; }
            while (b < m) { result.Add(new DiffLineModel { Type = DiffLineType.Added, NewLineNumber = b + 1, Content = newLines[b] }); added++; b++; }

            return (result, added, removed);
        }
    }

    // ===================== Cây kiến trúc + file =====================

    /// <summary>Một nút trong cây kiến trúc dự án — thư mục hoặc file mã nguồn. Kế thừa ViewModelBase
    /// để TreeView bind trực tiếp IsExpanded/IsSelected, đồng thời tách riêng Content (đã lưu) và
    /// DraftContent (đang soạn thảo, chưa commit) để có thể sửa NHIỀU file song song mà không bị mất
    /// dữ liệu khi chuyển qua lại giữa các file (khác bản trước chỉ theo dõi 1 file dang dở).</summary>
    public class SourceNodeModel : ViewModelBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public SourceNodeType Type { get; set; } = SourceNodeType.Folder;

        private string _content;
        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }

        private string _draftContent;
        /// <summary>Nội dung đang soạn thảo — mặc định bằng Content khi chưa từng chỉnh sửa.</summary>
        public string DraftContent
        {
            get => _draftContent ?? Content;
            set { _draftContent = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }

        public bool IsDirty => _draftContent != null && _draftContent != Content;

        public void DiscardDraft()
        {
            _draftContent = null;
            OnPropertyChanged(nameof(DraftContent));
            OnPropertyChanged(nameof(IsDirty));
        }

        /// <summary>Chốt bản nháp thành nội dung chính thức — gọi khi commit.</summary>
        public void CommitDraft()
        {
            if (_draftContent == null) return;
            Content = _draftContent;
            _draftContent = null;
            OnPropertyChanged(nameof(DraftContent));
            OnPropertyChanged(nameof(IsDirty));
        }

        public ObservableCollection<SourceNodeModel> Children { get; set; } = new();

        private bool _isExpanded = true;
        public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(); } }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        public bool IsFolder => Type == SourceNodeType.Folder;

        public string Extension => Type == SourceNodeType.File && !string.IsNullOrEmpty(Name) && Name.Contains('.')
            ? Name[(Name.LastIndexOf('.') + 1)..].ToLowerInvariant()
            : string.Empty;

        /// <summary>Sao chép sâu toàn bộ nhánh — dùng khi release sang môi trường khác.</summary>
        public SourceNodeModel DeepClone()
        {
            var clone = new SourceNodeModel
            {
                Name = Name,
                RelativePath = RelativePath,
                Type = Type,
                Content = Content,
                IsExpanded = IsExpanded
            };

            foreach (var child in Children)
                clone.Children.Add(child.DeepClone());

            return clone;
        }
    }

    public class SourceProjectOption
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }
        public ProjectRole MyRole { get; set; } = ProjectRole.Developer;
        public bool CanRelease => MyRole is ProjectRole.Owner or ProjectRole.TechnicalLeader or ProjectRole.ProjectManager;
        public string LocalPath { get; set; }
        public bool IsCloned => !string.IsNullOrWhiteSpace(LocalPath);
    }

    public class ReleaseLogModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public SourceEnvironment FromEnvironment { get; set; }
        public SourceEnvironment ToEnvironment { get; set; }
        public string ReleasedBy { get; set; }
        public DateTime ReleasedDate { get; set; } = DateTime.Now;
        public string Note { get; set; }
    }

    public class CodeIssueModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public int? LineNumber { get; set; }
        public CodeIssueSeverity Severity { get; set; } = CodeIssueSeverity.Medium;
        public CodeIssueStatus Status { get; set; } = CodeIssueStatus.Open;
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string LocationDisplay => LineNumber.HasValue ? $"{FileName} : dòng {LineNumber}" : FileName;
    }

    /// <summary>Không gian làm việc của MỘT môi trường (Dev/Test/Product) trong MỘT dự án: cây kiến
    /// trúc, danh sách file đang có thay đổi (DirtyFiles), lịch sử commit (Commits, gồm cả Edit/Release/
    /// Revert), thông báo lỗi và lịch sử release đến môi trường này.</summary>
    public class SourceEnvironmentWorkspace : ViewModelBase
    {
        public SourceEnvironment Environment { get; set; }

        public ObservableCollection<SourceNodeModel> RootNodes { get; set; } = new();

        private SourceNodeModel _selectedFile;
        public SourceNodeModel SelectedFile
        {
            get => _selectedFile;
            private set { _selectedFile = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedFile)); }
        }
        public bool HasSelectedFile => SelectedFile != null;

        public ObservableCollection<CodeIssueModel> Issues { get; set; } = new();
        public ObservableCollection<ReleaseLogModel> ReleaseLogs { get; set; } = new();

        /// <summary>Lịch sử commit theo môi trường này — hiển thị kiểu GitHub Desktop (tab "Lịch sử").</summary>
        public ObservableCollection<SourceCommitModel> Commits { get; set; } = new();

        /// <summary>File đang có thay đổi chưa commit — dùng cho tab "Thay đổi".</summary>
        public ObservableCollection<SourceNodeModel> DirtyFiles { get; set; } = new();

        private SourceCommitModel _selectedCommit;
        public SourceCommitModel SelectedCommit
        {
            get => _selectedCommit;
            private set { _selectedCommit = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedCommit)); }
        }
        public bool HasSelectedCommit => SelectedCommit != null;

        public int OpenIssueCount => Issues.Count(i => i.Status != CodeIssueStatus.Resolved);
        public ReleaseLogModel LatestRelease => ReleaseLogs.OrderByDescending(r => r.ReleasedDate).FirstOrDefault();

        /// <summary>Mở 1 file để xem/sửa — đóng khung xem diff commit (nếu đang mở) vì 2 khung không
        /// hiển thị đồng thời trong cùng khu vực nội dung chính.</summary>
        public void SetSelectedFile(SourceNodeModel node)
        {
            ClearTreeSelection(RootNodes);
            if (node != null) node.IsSelected = true;
            SelectedFile = node;
            SelectedCommit = null;
        }

        /// <summary>Chọn 1 commit để xem diff — đóng file editor đang mở (nếu có).</summary>
        public void SetSelectedCommit(SourceCommitModel commit)
        {
            foreach (var c in Commits) c.IsSelected = ReferenceEquals(c, commit);
            RefreshCommitsList();

            SelectedCommit = commit;
            ClearTreeSelection(RootNodes);
            _selectedFile = null;
            OnPropertyChanged(nameof(SelectedFile));
            OnPropertyChanged(nameof(HasSelectedFile));
        }

        private static void ClearTreeSelection(IEnumerable<SourceNodeModel> nodes)
        {
            foreach (var n in nodes)
            {
                n.IsSelected = false;
                if (n.Children.Count > 0) ClearTreeSelection(n.Children);
            }
        }

        /// <summary>Duyệt phẳng toàn bộ file (bỏ qua thư mục) trong cây kiến trúc.</summary>
        public IEnumerable<SourceNodeModel> FlattenFiles(IEnumerable<SourceNodeModel> nodes = null)
        {
            foreach (var n in nodes ?? RootNodes)
            {
                if (n.IsFolder) { foreach (var f in FlattenFiles(n.Children)) yield return f; }
                else yield return n;
            }
        }

        /// <summary>Ép làm mới danh sách file đang có thay đổi — gọi sau mỗi lần sửa/commit/huỷ nháp.</summary>
        public void RefreshDirtyFiles()
        {
            var dirty = FlattenFiles().Where(f => f.IsDirty).ToList();
            DirtyFiles.Clear();
            foreach (var f in dirty) DirtyFiles.Add(f);
        }

        /// <summary>Ép làm mới UI danh sách commit vì SourceCommitModel.IsSelected cần ItemsControl vẽ lại.</summary>
        private void RefreshCommitsList()
        {
            var current = Commits.ToList();
            Commits.Clear();
            foreach (var c in current) Commits.Add(c);
        }

        public void TouchIssues()
        {
            var current = Issues.ToList();
            Issues.Clear();
            foreach (var i in current) Issues.Add(i);
            OnPropertyChanged(nameof(OpenIssueCount));
        }
    }
}