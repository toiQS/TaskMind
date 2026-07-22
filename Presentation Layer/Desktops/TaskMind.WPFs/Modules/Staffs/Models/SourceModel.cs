using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>
    /// Module "Mã nguồn": PM/Technical leader/Owner có khả năng release mã nguồn theo luồng
    /// Dev -> Test -> Product. Nhân sự chọn dự án, chọn nơi lưu cục bộ nếu chưa clone mã nguồn về,
    /// sau đó xem cây kiến trúc (component tree) + mã nguồn theo từng môi trường, chỉnh sửa mã nguồn
    /// và tạo thông báo lỗi cần sửa.
    /// </summary>

    /// <summary>3 môi trường release mã nguồn.</summary>
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

    /// <summary>Một nút trong cây kiến trúc dự án (component tree) — thư mục hoặc file mã nguồn.
    /// Kế thừa ViewModelBase để TreeView có thể bind trực tiếp IsExpanded/IsSelected.</summary>
    public class SourceNodeModel : ViewModelBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public SourceNodeType Type { get; set; } = SourceNodeType.Folder;

        /// <summary>Nội dung mã nguồn — chỉ có ý nghĩa khi Type = File.</summary>
        public string Content { get; set; }

        public ObservableCollection<SourceNodeModel> Children { get; set; } = new();

        private bool _isExpanded = true;
        public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(); } }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        public bool IsFolder => Type == SourceNodeType.Folder;

        /// <summary>Phần mở rộng file (cs/xaml/json...), dùng để chọn icon phù hợp.</summary>
        public string Extension => Type == SourceNodeType.File && !string.IsNullOrEmpty(Name) && Name.Contains('.')
            ? Name[(Name.LastIndexOf('.') + 1)..].ToLowerInvariant()
            : string.Empty;

        /// <summary>Sao chép sâu toàn bộ nhánh — dùng khi release sang môi trường khác để tránh 2 môi
        /// trường tham chiếu chung 1 đối tượng cây (sửa ở Dev không được làm thay đổi Test/Product).</summary>
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

    /// <summary>Dự án khả dụng để xem/quản lý mã nguồn — tham chiếu ProjectModel (module Staffs) qua
    /// tên/ID, không phụ thuộc trực tiếp để tránh vòng lặp giữa 2 màn hình.</summary>
    public class SourceProjectOption
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }

        /// <summary>Vai trò của nhân sự hiện tại trong dự án này (mục 3) — quyết định quyền release.</summary>
        public ProjectRole MyRole { get; set; } = ProjectRole.Developer;

        /// <summary>Chỉ Owner/Technical leader/Project manager mới được release giữa các môi trường.</summary>
        public bool CanRelease => MyRole is ProjectRole.Owner or ProjectRole.TechnicalLeader or ProjectRole.ProjectManager;

        /// <summary>Thư mục lưu mã nguồn cục bộ — null/rỗng nếu nhân sự chưa clone mã nguồn về máy.</summary>
        public string LocalPath { get; set; }

        public bool IsCloned => !string.IsNullOrWhiteSpace(LocalPath);
    }

    /// <summary>Một lần release mã nguồn từ môi trường này sang môi trường kế tiếp.</summary>
    public class ReleaseLogModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public SourceEnvironment FromEnvironment { get; set; }
        public SourceEnvironment ToEnvironment { get; set; }
        public string ReleasedBy { get; set; }
        public DateTime ReleasedDate { get; set; } = DateTime.Now;
        public string Note { get; set; }
    }

    /// <summary>Thông báo lỗi trong mã nguồn cần sửa — gắn với 1 file cụ thể trong 1 môi trường,
    /// có thể kèm số dòng để định vị nhanh.</summary>
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
    /// trúc + file đang mở + trạng thái chỉnh sửa + thông báo lỗi + lịch sử release. Mỗi môi trường độc
    /// lập vì mã nguồn giữa các môi trường có thể lệch nhau (Test/Product chưa nhận bản release mới nhất).</summary>
    public class SourceEnvironmentWorkspace : ViewModelBase
    {
        public SourceEnvironment Environment { get; set; }

        public ObservableCollection<SourceNodeModel> RootNodes { get; set; } = new();

        private SourceNodeModel _selectedFile;
        public SourceNodeModel SelectedFile
        {
            get => _selectedFile;
            private set
            {
                _selectedFile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedFile));
                EditableContent = value?.Content ?? string.Empty;
                IsDirty = false;
            }
        }
        public bool HasSelectedFile => SelectedFile != null;

        private string _editableContent;
        public string EditableContent
        {
            get => _editableContent;
            set
            {
                _editableContent = value;
                OnPropertyChanged();
                IsDirty = SelectedFile != null && value != SelectedFile.Content;
            }
        }

        private bool _isDirty;
        public bool IsDirty { get => _isDirty; set { _isDirty = value; OnPropertyChanged(); } }

        /// <summary>Thông báo lỗi trong mã nguồn cần sửa, khai báo riêng theo từng môi trường.</summary>
        public ObservableCollection<CodeIssueModel> Issues { get; set; } = new();

        /// <summary>Lịch sử các lần release ĐẾN môi trường này (từ môi trường trước đó).</summary>
        public ObservableCollection<ReleaseLogModel> ReleaseLogs { get; set; } = new();

        public int OpenIssueCount => Issues.Count(i => i.Status != CodeIssueStatus.Resolved);

        public ReleaseLogModel LatestRelease => ReleaseLogs.OrderByDescending(r => r.ReleasedDate).FirstOrDefault();

        /// <summary>Chọn 1 file để xem/sửa, đồng thời cập nhật cờ IsSelected trên toàn cây để tô sáng
        /// đúng 1 dòng trong TreeView.</summary>
        public void SetSelectedFile(SourceNodeModel node)
        {
            ClearSelection(RootNodes);
            if (node != null) node.IsSelected = true;
            SelectedFile = node;
        }

        private static void ClearSelection(IEnumerable<SourceNodeModel> nodes)
        {
            foreach (var n in nodes)
            {
                n.IsSelected = false;
                if (n.Children.Count > 0) ClearSelection(n.Children);
            }
        }

        /// <summary>Ép làm mới UI danh sách thông báo lỗi vì CodeIssueModel không implement INotifyPropertyChanged.</summary>
        public void TouchIssues()
        {
            var current = Issues.ToList();
            Issues.Clear();
            foreach (var i in current) Issues.Add(i);

            OnPropertyChanged(nameof(OpenIssueCount));
        }
    }
}