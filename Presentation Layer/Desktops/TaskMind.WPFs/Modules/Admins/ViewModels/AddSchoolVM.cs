using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    /// <summary>
    /// ViewModel cho form "Thêm cơ sở đào tạo mới". Được SchoolVM khởi tạo và truyền vào
    /// 2 callback: onSaved (khi tạo thành công) và onCancel (khi huỷ/đóng form).
    /// </summary>
    public class AddSchoolVM : ViewModelBase
    {
        private readonly Action<SchoolModel> _onSaved;
        private readonly Action _onCancel;

        public AddSchoolModel Form { get; } = new AddSchoolModel();

        public string Name
        {
            get => Form.Name;
            set { Form.Name = value; OnPropertyChanged(); }
        }

        public string Field
        {
            get => Form.Field;
            set { Form.Field = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => Form.Email;
            set { Form.Email = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => Form.Phone;
            set { Form.Phone = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => Form.Address;
            set { Form.Address = value; OnPropertyChanged(); }
        }

        /// <summary>Danh sách gói dịch vụ để bind vào ComboBox.</summary>
        public string[] PackageOptions { get; } = { "Starter", "Pro", "Enterprise" };

        public string Package
        {
            get => Form.Package;
            set { Form.Package = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddSchoolVM(Action<SchoolModel> onSaved, Action onCancel)
        {
            _onSaved = onSaved;
            _onCancel = onCancel;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => _onCancel?.Invoke());
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Vui lòng nhập tên cơ sở đào tạo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Field))
            {
                ErrorMessage = "Vui lòng nhập lĩnh vực đào tạo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) ||
                !Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorMessage = "Email không hợp lệ.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Phone) && !Regex.IsMatch(Phone.Trim(), @"^[0-9+()\-\s]{8,15}$"))
            {
                ErrorMessage = "Số điện thoại không hợp lệ.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Package))
            {
                ErrorMessage = "Vui lòng chọn gói dịch vụ.";
                return false;
            }

            ErrorMessage = null;
            return true;
        }

        /// <summary>
        /// TODO: thay Task.Delay bằng gọi service thực tế (POST /schools) để tạo cơ sở đào tạo,
        /// cơ sở vừa tạo sẽ ở trạng thái Pending chờ Admin duyệt (mục 4.8 - Quản lý cơ sở đào tạo).
        /// </summary>
        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;
            await Task.Delay(400);

            var school = new SchoolModel
            {
                Id = "S" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                Name = Name.Trim(),
                Field = Field.Trim(),
                Email = Email.Trim(),
                Phone = Phone?.Trim(),
                Address = Address?.Trim(),
                Package = Package,
                Status = SchoolStatus.Pending,
                JoinedDate = DateTime.Now,
                TeacherCount = 0,
                CourseCount = 0,
                StudentCount = 0
            };

            IsBusy = false;
            _onSaved?.Invoke(school);
        }
    }
}