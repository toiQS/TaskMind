using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class TodoVM : ViewModelBase
    {
        // TODO: thay bằng tên nhân sự đang đăng nhập lấy từ phiên làm việc thực tế.
        private const string CurrentUserName = "Lê Thị Hoa";

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private TodoScope _currentScope = TodoScope.AssignedToMe;
        /// <summary>Thẻ đang xem: việc được giao cho mình hay việc mình đã giao cho người khác.</summary>
        public TodoScope CurrentScope
        {
            get => _currentScope;
            set { _currentScope = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private TodoStatus? _statusFilter;
        public TodoStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private TodoItemModel _selectedTodo;
        public TodoItemModel SelectedTodo
        {
            get => _selectedTodo;
            set { _selectedTodo = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedTodo)); }
        }
        public bool HasSelectedTodo => SelectedTodo != null;

        private string _commentInput;
        public string CommentInput { get => _commentInput; set { _commentInput = value; OnPropertyChanged(); } }

        /// <summary>True khi panel "Tạo/Chỉnh sửa công việc" đang mở (overlay ở TodoView).</summary>
        private bool _isAddingTodo;
        public bool IsAddingTodo { get => _isAddingTodo; set { _isAddingTodo = value; OnPropertyChanged(); } }

        private AddTodoVM _addTodoVM;
        public AddTodoVM AddTodoVM { get => _addTodoVM; set { _addTodoVM = value; OnPropertyChanged(); } }

        /// <summary>Toàn bộ công việc tải từ service (cả việc được giao lẫn việc đã giao cho người khác).</summary>
        public ObservableCollection<TodoItemModel> Todos { get; } = new();

        /// <summary>Danh sách sau khi áp dụng phạm vi thẻ + tìm kiếm/lọc + sắp xếp.</summary>
        public ObservableCollection<TodoItemModel> FilteredTodos { get; } = new();

        /// <summary>Danh mục dự án + thành viên, dùng để gán công việc khi tạo/sửa (mục yêu cầu
        /// "gán tên thành viên dự án").</summary>
        public ObservableCollection<TodoProjectOption> ProjectOptions { get; } = new();

        private IEnumerable<TodoItemModel> ScopedTodos =>
            CurrentScope == TodoScope.AssignedToMe
                ? Todos.Where(t => !t.IsCreatedByMe)
                : Todos.Where(t => t.IsCreatedByMe);

        // ===== Thống kê công việc (theo thẻ đang chọn) =====
        public int TotalCount => ScopedTodos.Count();
        public int InProgressCount => ScopedTodos.Count(t => t.Status == TodoStatus.InProgress);
        public int CompletedCount => ScopedTodos.Count(t => t.Status == TodoStatus.Completed);
        public int OverdueCount => ScopedTodos.Count(t => t.IsOverdue);

        // ===== Số lượng hiển thị trên badge của từng thẻ (không phụ thuộc thẻ đang chọn) =====
        public int AssignedToMeCount => Todos.Count(t => !t.IsCreatedByMe);
        public int CreatedByMeCount => Todos.Count(t => t.IsCreatedByMe);

        public ICommand RefreshCommand { get; }
        public ICommand SetScopeCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand MarkCompleteCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand CreateTodoCommand { get; }
        public ICommand EditTodoCommand { get; }

        public TodoVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            SetScopeCommand = new RelayCommand(p => CurrentScope = p is TodoScope s ? s : TodoScope.AssignedToMe);
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is TodoStatus s ? s : (TodoStatus?)null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; });
            OpenDetailCommand = new RelayCommand(p => SelectedTodo = p as TodoItemModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedTodo = null);
            MarkCompleteCommand = new RelayCommand(p => MarkComplete(p as TodoItemModel));
            AddCommentCommand = new RelayCommand(_ => AddComment(), _ => CanAddComment());
            CreateTodoCommand = new RelayCommand(_ => CreateTodo());
            EditTodoCommand = new RelayCommand(p => EditTodo(p as TodoItemModel));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/todos?scope=assigned|created và GET /me/projects/options
            // (danh sách dự án + thành viên mà nhân sự đang tham gia) thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            ProjectOptions.Clear();
            ProjectOptions.Add(new TodoProjectOption
            {
                ProjectName = "Hệ thống ERP nội bộ",
                MemberNames = new() { "Trần Văn Bình", "Lê Thị Hoa", "Nguyễn Văn A" }
            });
            ProjectOptions.Add(new TodoProjectOption
            {
                ProjectName = "Website thương mại điện tử ABC",
                MemberNames = new() { "Phạm Minh Tuấn", "Đỗ Thu Trang", "Lê Thị Hoa" }
            });

            Todos.Clear();

            var t1 = new TodoItemModel
            {
                Title = "Thiết kế API chấm công",
                Description = "Thiết kế các endpoint REST cho module chấm công: check-in, check-out, tổng hợp công.",
                ProjectName = "Hệ thống ERP nội bộ",
                AssigneeName = "Lê Thị Hoa",
                AssignedByName = "Trần Văn Bình",
                Status = TodoStatus.InProgress,
                Priority = TodoPriority.High,
                CreatedDate = DateTime.Now.AddDays(-5),
                DueDate = DateTime.Now.AddDays(2),
                IsCreatedByMe = false
            };
            t1.Comments.Add(new TodoCommentModel { Author = "Trần Văn Bình", Content = "Nhớ review kỹ phần phân quyền nhé.", CreatedDate = DateTime.Now.AddDays(-2) });
            t1.Logs.Add(new TodoLogModel { ChangedBy = "Trần Văn Bình", Description = "Đổi hạn hoàn thành sang 2 ngày nữa.", ChangedDate = DateTime.Now.AddDays(-3) });
            Todos.Add(t1);

            var t2 = new TodoItemModel
            {
                Title = "Viết test case cho module chấm công",
                Description = "Chuẩn bị bộ test case cho các luồng check-in/check-out bất thường.",
                ProjectName = "Hệ thống ERP nội bộ",
                AssigneeName = "Nguyễn Văn A",
                AssignedByName = "Lê Thị Hoa",
                Status = TodoStatus.NotStarted,
                Priority = TodoPriority.Medium,
                CreatedDate = DateTime.Now.AddDays(-1),
                DueDate = DateTime.Now.AddDays(5),
                IsCreatedByMe = true
            };
            Todos.Add(t2);

            var t3 = new TodoItemModel
            {
                Title = "Tối ưu truy vấn báo cáo tồn kho",
                Description = "Query báo cáo tồn kho đang chậm khi dữ liệu lớn, cần thêm index và tối ưu lại.",
                ProjectName = "Website thương mại điện tử ABC",
                AssigneeName = "Lê Thị Hoa",
                AssignedByName = "Phạm Minh Tuấn",
                Status = TodoStatus.Completed,
                Priority = TodoPriority.Medium,
                CreatedDate = DateTime.Now.AddDays(-12),
                DueDate = DateTime.Now.AddDays(-5),
                CompletedDate = DateTime.Now.AddDays(-6),
                IsCreatedByMe = false
            };
            t3.Logs.Add(new TodoLogModel { ChangedBy = "Lê Thị Hoa", Description = "Đánh dấu hoàn thành công việc.", ChangedDate = DateTime.Now.AddDays(-6) });
            Todos.Add(t3);

            var t4 = new TodoItemModel
            {
                Title = "Chuẩn bị demo milestone 2 cho ABC Corp",
                Description = "Chuẩn bị kịch bản và môi trường demo cho buổi nghiệm thu milestone 2.",
                ProjectName = "Website thương mại điện tử ABC",
                AssigneeName = "Đỗ Thu Trang",
                AssignedByName = "Lê Thị Hoa",
                Status = TodoStatus.InProgress,
                Priority = TodoPriority.Urgent,
                CreatedDate = DateTime.Now.AddDays(-3),
                DueDate = DateTime.Now.AddDays(-1), // đã quá hạn
                IsCreatedByMe = true
            };
            Todos.Add(t4);

            ApplyFilter();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = ScopedTodos;

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(t =>
                    t.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    t.ProjectName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (StatusFilter.HasValue)
                query = query.Where(t => t.Status == StatusFilter.Value);

            // Sắp xếp theo yêu cầu: việc chưa hoàn thành lên trước (ưu tiên hạn gần nhất), việc đã
            // hoàn thành xuống cuối danh sách (mới hoàn thành gần đây nhất lên trên trong nhóm đó).
            var incomplete = query.Where(t => t.Status != TodoStatus.Completed)
                                   .OrderBy(t => t.DueDate ?? DateTime.MaxValue);
            var completed = query.Where(t => t.Status == TodoStatus.Completed)
                                  .OrderByDescending(t => t.CompletedDate);

            FilteredTodos.Clear();
            foreach (var t in incomplete.Concat(completed))
                FilteredTodos.Add(t);

            RaiseCounters();
        }

        /// <summary>Đánh dấu hoàn thành — chỉ áp dụng một chiều, công việc đã hoàn thành sẽ bị khoá
        /// không cho chỉnh sửa/mở lại (đúng yêu cầu "cập nhật công việc... chưa hoàn thành").</summary>
        private void MarkComplete(TodoItemModel todo)
        {
            if (todo == null || !todo.CanEdit) return;

            // TODO: gọi service PATCH /todos/{id}/complete
            todo.Status = TodoStatus.Completed;
            todo.CompletedDate = DateTime.Now;

            todo.Logs.Add(new TodoLogModel
            {
                ChangedBy = CurrentUserName,
                Description = "Đánh dấu hoàn thành công việc.",
                ChangedDate = DateTime.Now
            });

            Touch();
        }

        private bool CanAddComment() => SelectedTodo != null && !string.IsNullOrWhiteSpace(CommentInput);

        private void AddComment()
        {
            if (!CanAddComment()) return;

            // TODO: gọi service POST /todos/{id}/comments
            SelectedTodo.Comments.Add(new TodoCommentModel
            {
                Author = CurrentUserName,
                Content = CommentInput.Trim()
            });

            CommentInput = string.Empty;
            Touch();
        }

        /// <summary>Mở panel "Tạo công việc mới", gán callback nhận TodoItemModel vừa tạo.</summary>
        private void CreateTodo()
        {
            SelectedTodo = null; // đóng panel chi tiết nếu đang mở, tránh chồng 2 overlay

            var vm = new AddTodoVM(ProjectOptions, CurrentUserName);

            vm.OnSaved = todo =>
            {
                // TODO: khi có service thật, có thể gọi lại LoadAsync() thay vì thêm trực tiếp vào danh sách cục bộ
                Todos.Insert(0, todo);
                ApplyFilter();

                IsAddingTodo = false;
                AddTodoVM = null;
            };
            vm.OnCancelled = () =>
            {
                IsAddingTodo = false;
                AddTodoVM = null;
            };

            AddTodoVM = vm;
            IsAddingTodo = true;
        }

        /// <summary>Mở panel chỉnh sửa công việc đã tạo trước đó — chỉ cho phép khi CanEdit = true.</summary>
        private void EditTodo(TodoItemModel todo)
        {
            if (todo == null || !todo.CanEdit) return;

            SelectedTodo = null;

            var vm = new AddTodoVM(ProjectOptions, CurrentUserName, todo);

            vm.OnSaved = _ =>
            {
                ApplyFilter();
                IsAddingTodo = false;
                AddTodoVM = null;
            };
            vm.OnCancelled = () =>
            {
                IsAddingTodo = false;
                AddTodoVM = null;
            };

            AddTodoVM = vm;
            IsAddingTodo = true;
        }

        /// <summary>Ép làm mới UI vì TodoItemModel không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();

            if (SelectedTodo != null)
            {
                var updated = SelectedTodo;
                SelectedTodo = null;
                SelectedTodo = updated;
            }
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(InProgressCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(OverdueCount));
            OnPropertyChanged(nameof(AssignedToMeCount));
            OnPropertyChanged(nameof(CreatedByMeCount));
        }
    }
}