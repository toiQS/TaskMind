using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Trạng thái công việc (mục 4.7 - quản lý công việc/task trong dự án).</summary>
    public enum TodoStatus
    {
        NotStarted,  // Chưa bắt đầu
        InProgress,  // Đang thực hiện
        Completed    // Hoàn thành
    }

    /// <summary>Mức độ ưu tiên công việc.</summary>
    public enum TodoPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    /// <summary>Phạm vi xem trên màn Công việc: việc được giao cho mình hay việc mình đã giao cho người khác.</summary>
    public enum TodoScope
    {
        AssignedToMe,
        CreatedByMe
    }

    /// <summary>Một dòng lịch sử chỉnh sửa công việc — tự động ghi lại mỗi khi công việc được cập nhật
    /// (đúng yêu cầu "chỉnh sửa công việc đã tạo trước đó và ghi lại log").</summary>
    public class TodoLogModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ChangedBy { get; set; }
        public string Description { get; set; }
        public DateTime ChangedDate { get; set; } = DateTime.Now;
    }

    /// <summary>Một bình luận về công việc được giao.</summary>
    public class TodoCommentModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string Initial => string.IsNullOrWhiteSpace(Author) ? "?" : Author.Trim()[0].ToString().ToUpper();
    }

    /// <summary>
    /// Công việc (task) cá nhân trong phạm vi 1 dự án — có thể do chính nhân sự tạo và gán cho thành
    /// viên khác trong dự án, hoặc do người khác giao cho mình. Chỉ được chỉnh sửa khi chưa hoàn thành,
    /// mỗi lần cập nhật sẽ ghi lại log; có thể trao đổi qua bình luận (mục 4.7).
    /// </summary>
    public class TodoItemModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public string ProjectName { get; set; }

        public string AssigneeName { get; set; }
        public string AssignedByName { get; set; }

        public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        /// <summary>True nếu công việc này do chính nhân sự hiện tại tạo ra và gán cho người khác
        /// (quyết định công việc thuộc thẻ "Việc của tôi" hay "Tôi đã giao").</summary>
        public bool IsCreatedByMe { get; set; }

        public ObservableCollection<TodoCommentModel> Comments { get; set; } = new();
        public ObservableCollection<TodoLogModel> Logs { get; set; } = new();

        /// <summary>Chỉ được chỉnh sửa/đánh dấu hoàn thành khi công việc chưa hoàn thành.</summary>
        public bool CanEdit => Status != TodoStatus.Completed;

        public bool IsOverdue => Status != TodoStatus.Completed && DueDate.HasValue && DueDate.Value.Date < DateTime.Now.Date;

        public string AssigneeInitial => string.IsNullOrWhiteSpace(AssigneeName) ? "?" : AssigneeName.Trim()[0].ToString().ToUpper();
        public int CommentCount => Comments?.Count ?? 0;
        public int LogCount => Logs?.Count ?? 0;
    }

    /// <summary>Tuỳ chọn dự án rút gọn (tên + danh sách thành viên) dùng cho form tạo/gán công việc —
    /// tham chiếu dữ liệu tương ứng ProjectVM (module Staffs) mà không phụ thuộc trực tiếp vào VM đó.</summary>
    public class TodoProjectOption
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }
        public List<string> MemberNames { get; set; } = new();
    }
}