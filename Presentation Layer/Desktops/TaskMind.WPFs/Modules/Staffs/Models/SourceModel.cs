using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>3 môi trường mã nguồn hiển thị dạng 3 thẻ (tab) trên màn "Mã nguồn": mỗi môi trường có
    /// đường dẫn lưu cục bộ, cây kiến trúc dự án và danh sách báo lỗi (issue) riêng.</summary>
    public enum SourceEnvironment
    {
        Development,
        Testing,
        Production
    }

    /// <summary>Mức độ nghiêm trọng của một báo lỗi mã nguồn.</summary>
    public enum IssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>Trạng thái xử lý một báo lỗi mã nguồn.</summary>
    public enum IssueStatus
    {
        Open,
        InProgress,
        Resolved
    }

    /// <summary>Một nút trong cây kiến trúc dự án (component tree) — có thể là thư mục (IsFolder = true,
    /// chỉ dùng Children) hoặc file mã nguồn (IsFolder = false, dùng Content để hiển thị/chỉnh sửa).</summary>
    public class SourceFileNode
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        /// <summary>Đường dẫn tương đối trong repo, dùng làm khoá tra cứu nội dung và gắn báo lỗi
        /// (VD: "src/Services/AttendanceService.cs").</summary>
        public string FullPath { get; set; }

        public bool IsFolder { get; set; }

        public ObservableCollection<SourceFileNode> Children { get; set; } = new();

        /// <summary>Nội dung mã nguồn — chỉ có ý nghĩa khi IsFolder = false.</summary>
        public string Content { get; set; }

        public string Extension => IsFolder || string.IsNullOrEmpty(Name)
            ? null
            : Path.GetExtension(Name).TrimStart('.').ToUpperInvariant();
    }

    /// <summary>Một báo lỗi mã nguồn gắn với 1 file (và tuỳ chọn 1 dòng cụ thể) trong 1 môi trường
    /// (đúng yêu cầu "tạo thông báo lỗi trong mã nguồn cần sửa").</summary>
    public class SourceIssueModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FilePath { get; set; }
        public int? LineNumber { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public IssueSeverity Severity { get; set; } = IssueSeverity.Medium;
        public IssueStatus Status { get; set; } = IssueStatus.Open;

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public SourceEnvironment Environment { get; set; }

        public string LocationDisplay => LineNumber.HasValue
            ? $"{FilePath} (dòng {LineNumber})"
            : FilePath;
    }

    /// <summary>Dữ liệu mã nguồn của MỘT môi trường cụ thể trong MỘT dự án: đường dẫn lưu cục bộ (nếu
    /// đã cấu hình/clone), cây kiến trúc và danh sách báo lỗi riêng của môi trường đó.</summary>
    public class SourceEnvironmentData
    {
        public SourceEnvironment Environment { get; set; }

        /// <summary>Nơi lưu mã nguồn cục bộ trên máy — null/rỗng nếu chưa clone/cấu hình (đúng yêu cầu
        /// "lựa chọn nơi lưu cục bộ nếu chưa clone mã nguồn về").</summary>
        public string LocalPath { get; set; }

        public bool HasLocalPath => !string.IsNullOrWhiteSpace(LocalPath);

        public ObservableCollection<SourceFileNode> RootNodes { get; set; } = new();
        public ObservableCollection<SourceIssueModel> Issues { get; set; } = new();
    }

    /// <summary>Một dự án hiển thị trên màn "Mã nguồn" — chọn dự án trước khi xem mã nguồn (đúng yêu
    /// cầu "chọn dự án trước khi hiển thị mã nguồn"). Mỗi dự án có dữ liệu độc lập cho cả 3 môi trường.</summary>
    public class SourceProjectOption
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }

        /// <summary>Địa chỉ repo — hiển thị tham khảo và dùng khi clone lần đầu vào nơi lưu cục bộ.</summary>
        public string RepositoryUrl { get; set; }

        public Dictionary<SourceEnvironment, SourceEnvironmentData> EnvironmentData { get; set; } = new()
        {
            { SourceEnvironment.Development, new SourceEnvironmentData { Environment = SourceEnvironment.Development } },
            { SourceEnvironment.Testing, new SourceEnvironmentData { Environment = SourceEnvironment.Testing } },
            { SourceEnvironment.Production, new SourceEnvironmentData { Environment = SourceEnvironment.Production } }
        };
    }
}