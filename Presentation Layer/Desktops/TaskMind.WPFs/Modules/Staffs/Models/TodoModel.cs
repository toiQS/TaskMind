using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Trạng thái công việc.</summary>
    public enum TodoStatus
    {
        NotStarted,   // Chưa bắt đầu
        InProgress,   // Đang thực hiện
        Completed     // Hoàn thành
    }

    /// <summary>Mức độ ưu tiên công việc.</summary>
    public enum TodoPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    /// <summary>Một dòng nhật ký thay đổi của công việc (ai làm gì, khi nào).</summary>
    public class TodoLogModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ActorName { get; set; }
        public string Action { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }

    /// <summary>Một bình luận trao đổi quanh công việc được giao.</summary>
    public class TodoCommentModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; } = DateTime.Now;

        public string Initial => string.IsNullOrWhiteSpace(AuthorName) ? "?" : AuthorName.Trim()[0].ToString().ToUpper();
    }

    /// <summary>
    /// Một công việc (todo/task) gán cho thành viên dự án.
    /// TaskMind.docx mục 4.7 (Quản lý dự án trực thuộc công ty) — công việc/task nằm trong phạm vi 1 dự án.
    /// </summary>
    public class TodoItemModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        /// <summary>Dự án chứa công việc này (tham chiếu ProjectModel theo tên, mục 4.7).</summary>
        public string ProjectName { get; set; }

        /// <summary>Thành viên dự án được gán thực hiện công việc.</summary>
        public string AssigneeName { get; set; }

        public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        /// <summary>Dùng để tô sáng card đang được chọn trong danh sách (không phải INotifyPropertyChanged).</summary>
        public bool IsSelected { get; set; }

        public List<TodoCommentModel> Comments { get; set; } = new();
        public List<TodoLogModel> Logs { get; set; } = new();

        public string AssigneeInitial => string.IsNullOrWhiteSpace(AssigneeName) ? "?" : AssigneeName.Trim()[0].ToString().ToUpper();

        /// <summary>Chỉ công việc chưa hoàn thành mới được phép chỉnh sửa nội dung.</summary>
        public bool CanEdit => Status != TodoStatus.Completed;

        public bool IsOverdue => Status != TodoStatus.Completed && DueDate.HasValue && DueDate.Value.Date < DateTime.Now.Date;

        public int CommentCount => Comments?.Count ?? 0;

        /// <summary>Khoá sắp xếp: chưa hoàn thành lên trước (theo hạn gần nhất), hoàn thành xuống cuối (theo ngày xong gần nhất).</summary>
        public DateTime SortKey => Status == TodoStatus.Completed
            ? (CompletedDate ?? CreatedDate)
            : (DueDate ?? DateTime.MaxValue);
    }
}