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
    /// <summary>
    /// ViewModel cho form "Tạo công việc mới" / "Chỉnh sửa công việc" (mục 4.7). Dùng chung một lớp
    /// cho cả hai chế độ: truyền <paramref name="editingTodo"/> (null) khi tạo mới, hoặc một
    /// <see cref="TodoItemModel"/> có thật khi chỉnh sửa — chỉnh sửa chỉ được phép khi CanEdit = true
    /// (TodoVM đã chặn việc mở form sửa với công việc đã hoàn thành).
    /// </summary>
    public class AddTodoVM : ViewModelBase
    {
        private readonly string _currentUserName;
        private readonly TodoItemModel _editingTodo;

        /// <summary>True nếu đang ở chế độ chỉnh sửa công việc đã tồn tại.</summary>
        public bool IsEditMode => _editingTodo != null;

        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        /// <summary>Danh mục dự án khả dụng để gán công việc (tham chiếu mục 4.5/4.7 — chỉ nhân sự
        /// trong dự án mới có thể được gán).</summary>
        public ObservableCollection<TodoProjectOption> ProjectOptions { get; }

        private TodoProjectOption _selectedProject;
        public TodoProjectOption SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
                RefreshAvailableAssignees();
            }
        }

        /// <summary>Danh sách thành viên của dự án đang chọn, dùng để chọn người được gán công việc.</summary>
        public ObservableCollection<string> AvailableAssignees { get; } = new();

        private string _assigneeName;
        public string AssigneeName { get => _assigneeName; set { _assigneeName = value; OnPropertyChanged(); } }

        private TodoPriority _priority = TodoPriority.Medium;
        public TodoPriority Priority { get => _priority; set { _priority = value; OnPropertyChanged(); } }

        private DateTime? _dueDate = DateTime.Now.AddDays(7);
        public DateTime? DueDate { get => _dueDate; set { _dueDate = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>TodoVM gán 2 callback này khi mở panel, để nhận TodoItemModel vừa tạo/sửa hoặc đóng panel khi huỷ.</summary>
        public Action<TodoItemModel> OnSaved { get; set; }
        public Action OnCancelled { get; set; }

        public AddTodoVM(IEnumerable<TodoProjectOption> projectOptions, string currentUserName, TodoItemModel editingTodo = null)
        {
            _currentUserName = currentUserName;
            _editingTodo = editingTodo;

            ProjectOptions = new ObservableCollection<TodoProjectOption>(projectOptions ?? Enumerable.Empty<TodoProjectOption>());

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());

            if (editingTodo != null)
            {
                // Chế độ chỉnh sửa: prefill toàn bộ dữ liệu từ công việc đã có.
                Title = editingTodo.Title;
                Description = editingTodo.Description;
                Priority = editingTodo.Priority;
                DueDate = editingTodo.DueDate;

                SelectedProject = ProjectOptions.FirstOrDefault(p =>
                    string.Equals(p.ProjectName, editingTodo.ProjectName, StringComparison.OrdinalIgnoreCase))
                    ?? ProjectOptions.FirstOrDefault();

                // Gán sau khi SelectedProject đã nạp AvailableAssignees, tránh bị RefreshAvailableAssignees xoá mất.
                AssigneeName = editingTodo.AssigneeName;
            }
            else
            {
                SelectedProject = ProjectOptions.FirstOrDefault();
            }
        }

        private void RefreshAvailableAssignees()
        {
            var previousAssignee = AssigneeName;

            AvailableAssignees.Clear();
            if (SelectedProject?.MemberNames != null)
            {
                foreach (var name in SelectedProject.MemberNames)
                    AvailableAssignees.Add(name);
            }

            // Giữ lại người đang được gán nếu vẫn thuộc dự án mới chọn, ngược lại xoá để tránh gán nhầm người
            // không thuộc dự án (mục 4.7 — thành viên dự án phải là nhân sự có thật trong dự án đó).
            AssigneeName = previousAssignee != null && AvailableAssignees.Contains(previousAssignee)
                ? previousAssignee
                : null;
        }

        private bool Validate()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Vui lòng nhập tên công việc.";
                return false;
            }

            if (SelectedProject == null)
            {
                ErrorMessage = "Vui lòng chọn dự án.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(AssigneeName))
            {
                ErrorMessage = "Vui lòng chọn người được gán công việc.";
                return false;
            }

            if (DueDate.HasValue && DueDate.Value.Date < DateTime.Now.Date && !IsEditMode)
            {
                ErrorMessage = "Hạn hoàn thành không được ở trong quá khứ.";
                return false;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;

            // TODO: gọi service POST /todos (tạo mới) hoặc PATCH /todos/{id} (chỉnh sửa) thay cho việc
            // cập nhật trực tiếp đối tượng cục bộ ở đây.
            await Task.Delay(300);

            if (IsEditMode)
            {
                _editingTodo.Title = Title.Trim();
                _editingTodo.Description = Description?.Trim();
                _editingTodo.ProjectName = SelectedProject.ProjectName;
                _editingTodo.AssigneeName = AssigneeName;
                _editingTodo.Priority = Priority;
                _editingTodo.DueDate = DueDate;

                // Ghi log chỉnh sửa tự động (đúng yêu cầu "chỉnh sửa công việc đã tạo và ghi lại log").
                _editingTodo.Logs.Add(new TodoLogModel
                {
                    ChangedBy = _currentUserName,
                    Description = "Cập nhật thông tin công việc.",
                    ChangedDate = DateTime.Now
                });

                IsBusy = false;
                OnSaved?.Invoke(_editingTodo);
                return;
            }

            var todo = new TodoItemModel
            {
                Title = Title.Trim(),
                Description = Description?.Trim(),
                ProjectName = SelectedProject.ProjectName,
                AssigneeName = AssigneeName,
                AssignedByName = _currentUserName,
                Priority = Priority,
                DueDate = DueDate,
                Status = TodoStatus.NotStarted,
                IsCreatedByMe = true
            };

            IsBusy = false;
            OnSaved?.Invoke(todo);
        }
    }
}