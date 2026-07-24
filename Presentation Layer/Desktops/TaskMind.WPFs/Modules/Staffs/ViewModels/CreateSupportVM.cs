using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    /// <summary>ViewModel cho form "Tạo yêu cầu hỗ trợ mới" gửi đến Admin công ty.</summary>
    public class CreateSupportVM : ViewModelBase
    {
        private readonly string _currentUserName;

        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private SupportCategory _category = SupportCategory.Other;
        public SupportCategory Category { get => _category; set { _category = value; OnPropertyChanged(); } }

        private TodoPriority _priority = TodoPriority.Medium;
        public TodoPriority Priority { get => _priority; set { _priority = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>SupportVM gán 2 callback này khi mở panel, để nhận SupportRequestModel vừa tạo hoặc đóng panel khi huỷ.</summary>
        public Action<SupportRequestModel> OnSaved { get; set; }
        public Action OnCancelled { get; set; }

        public CreateSupportVM(string currentUserName)
        {
            _currentUserName = currentUserName;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());
        }

        private bool Validate()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Vui lòng nhập tiêu đề yêu cầu.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Vui lòng mô tả chi tiết vấn đề cần hỗ trợ.";
                return false;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;

            // TODO: gọi service POST /me/support-requests thay cho việc tạo trực tiếp đối tượng cục bộ.
            await Task.Delay(300);

            var request = new SupportRequestModel
            {
                Title = Title.Trim(),
                Description = Description.Trim(),
                Category = Category,
                Priority = Priority,
                Status = SupportStatus.Pending,
                CreatedDate = DateTime.Now
            };

            IsBusy = false;
            OnSaved?.Invoke(request);
        }
    }
}