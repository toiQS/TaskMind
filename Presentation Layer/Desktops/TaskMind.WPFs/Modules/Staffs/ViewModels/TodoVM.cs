using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class TodoVM : ViewModelBase
    {
        /// <summary>Người dùng hiện tại (giả lập, dùng cho tác giả bình luận/nhật ký). TODO: lấy từ phiên đăng nhập thật.</summary>
        private const string CurrentUserName = "Trần Văn Bình";

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private TodoStatus? _statusFilter;
        public TodoStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private TodoPriority? _priorityFilter;
        public TodoPriority? PriorityFilter { get => _priorityFilter; set { _priorityFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private TodoItemModel _selectedTodo;
        public TodoItemModel SelectedTodo
        {
            get => _selectedTodo;
            set
            {
                foreach (var t in Todos)
                    t.IsSelected = ReferenceEquals(t, value);

                _selectedTodo = value;
                IsEditing = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedTodo));
                RefreshList();
            }
        }
        public bool HasSelectedTodo => SelectedTodo != null;

        /// <summary>Toàn bộ công việc tải từ service.</summary>
        public ObservableCollection<TodoItemModel> Todos { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc + sắp xếp (chưa hoàn thành trước, hoàn thành sau).</summary>
        public ObservableCollection<TodoItemModel> FilteredTodos { get; } = new();

        /// <summary>Danh sách dự án để gán khi tạo công việc mới (tham chiếu mục 4.7).</summary>
        public List<string> ProjectOptions { get; } = new()
        {
            "Hệ thống ERP nội bộ",
            "Website thương mại điện tử ABC",
            "App quản lý kho",
            "Nền tảng học trực tuyến"
        };

        /// <summary>Danh sách thành viên dự án để gán công việc.</summary>
        public List<string> MemberOptions { get; } = new()
        {
            "Trần Văn Bình",
            "Lê Thị Hoa",
            "Nguyễn Văn A",
            "Phạm Minh Tuấn",
            "Đỗ Thu Trang",
            "Phạm Thị D"
        };

        public int TotalCount => Todos.Count;
        public int NotStartedCount => Todos.Count(t => t.Status == TodoStatus.NotStarted);
        public int InProgressCount => Todos.Count(t => t.Status == TodoStatus.InProgress);
        public int CompletedCount => Todos.Count(t => t.Status == TodoStatus.Completed);
        public int OverdueCount => Todos.Count(t => t.IsOverdue);

        // ===== Tạo công việc mới =====
        private bool _isCreating;
        public bool IsCreating { get => _isCreating; set { _isCreating = value; OnPropertyChanged(); } }

        private string _newTitle;
        public string NewTitle { get => _newTitle; set { _newTitle = value; OnPropertyChanged(); } }

        private string _newDescription;
        public string NewDescription { get => _newDescription; set { _newDescription = value; OnPropertyChanged(); } }

        private string _newProjectName;
        public string NewProjectName { get => _newProjectName; set { _newProjectName = value; OnPropertyChanged(); } }

        private string _newAssigneeName;
        public string NewAssigneeName { get => _newAssigneeName; set { _newAssigneeName = value; OnPropertyChanged(); } }

        private DateTime? _newDueDate = DateTime.Now.AddDays(3);
        public DateTime? NewDueDate { get => _newDueDate; set { _newDueDate = value; OnPropertyChanged(); } }

        private TodoPriority _newPriority = TodoPriority.Medium;
        public TodoPriority NewPriority { get => _newPriority; set { _newPriority = value; OnPropertyChanged(); } }

        private string _createError;
        public string CreateError { get => _createError; set { _createError = value; OnPropertyChanged(); } }

        // ===== Chỉnh sửa công việc đang chọn (chỉ khi chưa hoàn thành) =====
        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

        private string _editTitle;
        public string EditTitle { get => _editTitle; set { _editTitle = value; OnPropertyChanged(); } }

        private string _editDescription;
        public string EditDescription { get => _editDescription; set { _editDescription = value; OnPropertyChanged(); } }

        private DateTime? _editDueDate;
        public DateTime? EditDueDate { get => _editDueDate; set { _editDueDate = value; OnPropertyChanged(); } }

        private TodoPriority _editPriority;
        public TodoPriority EditPriority { get => _editPriority; set { _editPriority = value; OnPropertyChanged(); } }

        // ===== Bình luận =====
        private string _commentInput;
        public string CommentInput { get => _commentInput; set { _commentInput = value; OnPropertyChanged(); } }

        public ICommand RefreshCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand SetPriorityFilterCommand { get; }

        public ICommand OpenCreateCommand { get; }
        public ICommand CancelCreateCommand { get; }
        public ICommand SaveCreateCommand { get; }

        public ICommand EditCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelEditCommand { get; }

        public ICommand StartCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand ReopenCommand { get; }
        public ICommand DeleteCommand { get; }

        public ICommand AddCommentCommand { get; }

        public TodoVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            OpenDetailCommand = new RelayCommand(p => SelectedTodo = p as TodoItemModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedTodo = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; PriorityFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is TodoStatus s ? s : (TodoStatus?)null);
            SetPriorityFilterCommand = new RelayCommand(p => PriorityFilter = p is TodoPriority pr ? pr : (TodoPriority?)null);

            OpenCreateCommand = new RelayCommand(_ => OpenCreate());
            CancelCreateCommand = new RelayCommand(_ => IsCreating = false);
            SaveCreateCommand = new RelayCommand(async _ => await SaveCreateAsync());

            EditCommand = new RelayCommand(_ => StartEdit(), _ => SelectedTodo?.CanEdit == true);
            SaveEditCommand = new RelayCommand(async _ => await SaveEditAsync());
            CancelEditCommand = new RelayCommand(_ => IsEditing = false);

            StartCommand = new RelayCommand(p => ChangeStatus(p as TodoItemModel, TodoStatus.InProgress));
            CompleteCommand = new RelayCommand(p => ChangeStatus(p as TodoItemModel, TodoStatus.Completed));
            ReopenCommand = new RelayCommand(p => ChangeStatus(p as TodoItemModel, TodoStatus.NotStarted));
            DeleteCommand = new RelayCommand(p => Delete(p as TodoItemModel));

            AddCommentCommand = new RelayCommand(_ => AddComment(), _ => CanAddComment());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /staff/{staffId}/todos thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Todos.Clear();

            var t1 = new TodoItemModel
            {
                Title = "Viết API chấm công nhân sự",
                Description = "Xây dựng endpoint check-in/check-out và tổng hợp giờ công theo ngày cho module ERP.",
                ProjectName = "Hệ thống ERP nội bộ",
                AssigneeName = "Trần Văn Bình",
                Status = TodoStatus.InProgress,
                Priority = TodoPriority.High,
                CreatedDate = DateTime.Now.AddDays(-4),
                DueDate = DateTime.Now.AddDays(1)
            };
            t1.Logs.Add(new TodoLogModel { ActorName = "Lê Thị Hoa", Action = "Tạo công việc và gán cho Trần Văn Bình", Time = t1.CreatedDate });
            t1.Logs.Add(new TodoLogModel { ActorName = "Trần Văn Bình", Action = "Chuyển trạng thái sang Đang thực hiện", Time = DateTime.Now.AddDays(-2) });
            t1.Comments.Add(new TodoCommentModel { AuthorName = "Lê Thị Hoa", Content = "Nhớ viết thêm test case cho trường hợp check-in trễ nhé.", SentDate = DateTime.Now.AddDays(-1) });
            Todos.Add(t1);

            var t2 = new TodoItemModel
            {
                Title = "Viết test case module thanh toán",
                Description = "Kiểm thử luồng thanh toán theo milestone cho dự án Website TMĐT ABC.",
                ProjectName = "Website thương mại điện tử ABC",
                AssigneeName = "Đỗ Thu Trang",
                Status = TodoStatus.NotStarted,
                Priority = TodoPriority.Medium,
                CreatedDate = DateTime.Now.AddDays(-1),
                DueDate = DateTime.Now.AddDays(-1) // đã quá hạn để minh hoạ cảnh báo overdue
            };
            t2.Logs.Add(new TodoLogModel { ActorName = "Phạm Minh Tuấn", Action = "Tạo công việc và gán cho Đỗ Thu Trang", Time = t2.CreatedDate });
            Todos.Add(t2);

            var t3 = new TodoItemModel
            {
                Title = "Review PR module chấm công",
                Description = "Review code, kiểm tra chuẩn coding convention và hiệu năng truy vấn.",
                ProjectName = "Hệ thống ERP nội bộ",
                AssigneeName = "Lê Thị Hoa",
                Status = TodoStatus.Completed,
                Priority = TodoPriority.Medium,
                CreatedDate = DateTime.Now.AddDays(-6),
                DueDate = DateTime.Now.AddDays(-3),
                CompletedDate = DateTime.Now.AddDays(-3)
            };
            t3.Logs.Add(new TodoLogModel { ActorName = "Trần Văn Bình", Action = "Tạo công việc và gán cho Lê Thị Hoa", Time = t3.CreatedDate });
            t3.Logs.Add(new TodoLogModel { ActorName = "Lê Thị Hoa", Action = "Đánh dấu hoàn thành", Time = t3.CompletedDate.Value });
            Todos.Add(t3);

            var t4 = new TodoItemModel
            {
                Title = "Thiết kế màn hình đăng ký thực tập sinh QA/QC",
                Description = "Thiết kế UI/UX màn onboarding cho thực tập sinh mới theo phong cách hiện có.",
                ProjectName = "App quản lý kho",
                AssigneeName = "Phạm Thị D",
                Status = TodoStatus.NotStarted,
                Priority = TodoPriority.Low,
                CreatedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7)
            };
            t4.Logs.Add(new TodoLogModel { ActorName = "Trần Văn Bình", Action = "Tạo công việc và gán cho Phạm Thị D", Time = t4.CreatedDate });
            Todos.Add(t4);

            var t5 = new TodoItemModel
            {
                Title = "Chuẩn bị tài liệu bàn giao milestone 2",
                Description = "Tổng hợp tài liệu kỹ thuật và biên bản nghiệm thu gửi đối tác ABC Corp.",
                ProjectName = "Website thương mại điện tử ABC",
                AssigneeName = "Phạm Minh Tuấn",
                Status = TodoStatus.Completed,
                Priority = TodoPriority.Urgent,
                CreatedDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(-8),
                CompletedDate = DateTime.Now.AddDays(-8)
            };
            t5.Logs.Add(new TodoLogModel { ActorName = "Phạm Minh Tuấn", Action = "Tạo công việc", Time = t5.CreatedDate });
            t5.Logs.Add(new TodoLogModel { ActorName = "Phạm Minh Tuấn", Action = "Đánh dấu hoàn thành", Time = t5.CompletedDate.Value });
            Todos.Add(t5);

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Todos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(t =>
                    t.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    t.ProjectName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    t.AssigneeName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (StatusFilter.HasValue) query = query.Where(t => t.Status == StatusFilter.Value);
            if (PriorityFilter.HasValue) query = query.Where(t => t.Priority == PriorityFilter.Value);

            // Sắp xếp: chưa hoàn thành trước (theo hạn gần nhất), hoàn thành xuống cuối (theo ngày xong gần nhất)
            FilteredTodos.Clear();
            foreach (var t in query.OrderBy(t => t.Status == TodoStatus.Completed)
                                    .ThenBy(t => t.Status == TodoStatus.Completed ? DateTime.MaxValue - t.SortKey : t.SortKey - DateTime.MinValue))
                FilteredTodos.Add(t);
        }

        private void OpenCreate()
        {
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            NewProjectName = ProjectOptions.FirstOrDefault();
            NewAssigneeName = MemberOptions.FirstOrDefault();
            NewDueDate = DateTime.Now.AddDays(3);
            NewPriority = TodoPriority.Medium;
            CreateError = string.Empty;
            IsCreating = true;
        }

        private async Task SaveCreateAsync()
        {
            CreateError = string.Empty;

            if (string.IsNullOrWhiteSpace(NewTitle))
            {
                CreateError = "Vui lòng nhập tên công việc.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewAssigneeName))
            {
                CreateError = "Vui lòng gán công việc cho một thành viên dự án.";
                return;
            }

            IsBusy = true;

            var todo = new TodoItemModel
            {
                Title = NewTitle.Trim(),
                Description = NewDescription?.Trim(),
                ProjectName = NewProjectName,
                AssigneeName = NewAssigneeName,
                Priority = NewPriority,
                DueDate = NewDueDate,
                Status = TodoStatus.NotStarted
            };
            todo.Logs.Add(new TodoLogModel
            {
                ActorName = CurrentUserName,
                Action = $"Tạo công việc và gán cho {NewAssigneeName}",
                Time = todo.CreatedDate
            });

            // TODO: gọi service POST /staff/todos (todo) thay cho thêm trực tiếp vào danh sách bên dưới
            await Task.Delay(300);

            Todos.Insert(0, todo);
            ApplyFilter();
            RaiseCounters();

            IsBusy = false;
            IsCreating = false;
        }

        private void StartEdit()
        {
            if (SelectedTodo?.CanEdit != true) return;

            EditTitle = SelectedTodo.Title;
            EditDescription = SelectedTodo.Description;
            EditDueDate = SelectedTodo.DueDate;
            EditPriority = SelectedTodo.Priority;
            IsEditing = true;
        }

        private async Task SaveEditAsync()
        {
            if (SelectedTodo?.CanEdit != true) return;

            if (string.IsNullOrWhiteSpace(EditTitle))
                return;

            IsBusy = true;

            // TODO: gọi service PATCH /staff/todos/{id}
            await Task.Delay(300);

            SelectedTodo.Title = EditTitle.Trim();
            SelectedTodo.Description = EditDescription?.Trim();
            SelectedTodo.DueDate = EditDueDate;
            SelectedTodo.Priority = EditPriority;
            SelectedTodo.Logs.Add(new TodoLogModel { ActorName = CurrentUserName, Action = "Cập nhật nội dung công việc" });

            IsBusy = false;
            IsEditing = false;
            Touch();
        }

        private void ChangeStatus(TodoItemModel todo, TodoStatus status)
        {
            if (todo == null) return;

            // TODO: gọi service PATCH /staff/todos/{id}/status
            todo.Status = status;
            todo.CompletedDate = status == TodoStatus.Completed ? DateTime.Now : null;

            var actionText = status switch
            {
                TodoStatus.InProgress => "Chuyển trạng thái sang Đang thực hiện",
                TodoStatus.Completed => "Đánh dấu hoàn thành",
                TodoStatus.NotStarted => "Mở lại công việc (chưa bắt đầu)",
                _ => "Cập nhật trạng thái"
            };
            todo.Logs.Add(new TodoLogModel { ActorName = CurrentUserName, Action = actionText });

            IsEditing = false;
            Touch();
        }

        private void Delete(TodoItemModel todo)
        {
            if (todo == null) return;

            // TODO: gọi service DELETE /staff/todos/{id}
            Todos.Remove(todo);

            if (ReferenceEquals(SelectedTodo, todo))
                SelectedTodo = null;

            ApplyFilter();
            RaiseCounters();
        }

        private bool CanAddComment() => SelectedTodo != null && !string.IsNullOrWhiteSpace(CommentInput);

        private void AddComment()
        {
            if (!CanAddComment()) return;

            // TODO: gọi service POST /staff/todos/{id}/comments (có thể kèm bắn Notification, mục 5.3)
            SelectedTodo.Comments.Add(new TodoCommentModel
            {
                AuthorName = CurrentUserName,
                Content = CommentInput.Trim()
            });

            CommentInput = string.Empty;
            Touch();
        }

        /// <summary>Ép làm mới UI vì các model không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();

            if (SelectedTodo != null)
            {
                var updated = SelectedTodo;
                _selectedTodo = null;
                OnPropertyChanged(nameof(SelectedTodo));
                _selectedTodo = updated;
                OnPropertyChanged(nameof(SelectedTodo));
            }
        }

        /// <summary>Ép ItemsControl render lại container để DataTrigger đọc lại IsSelected mới nhất.</summary>
        private void RefreshList()
        {
            var current = FilteredTodos.ToList();
            FilteredTodos.Clear();
            foreach (var t in current)
                FilteredTodos.Add(t);
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(NotStartedCount));
            OnPropertyChanged(nameof(InProgressCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(OverdueCount));
        }
    }
}