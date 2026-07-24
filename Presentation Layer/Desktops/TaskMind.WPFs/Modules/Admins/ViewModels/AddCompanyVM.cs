using System.Text.RegularExpressions;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    /// <summary>
    /// ViewModel cho form "Thêm công ty mới". Được CompanyVM khởi tạo và truyền vào
    /// 2 callback: onSaved (khi tạo thành công) và onCancel (khi huỷ/đóng form).
    /// </summary>
    public class AddCompanyVM : ViewModelBase
    {
        private readonly Action<CompanyModel> _onSaved;
        private readonly Action _onCancel;

        public AddCompanyModel Form { get; } = new AddCompanyModel();

        public string Name
        {
            get => Form.Name;
            set { Form.Name = value; OnPropertyChanged(); }
        }

        public string TaxCode
        {
            get => Form.TaxCode;
            set { Form.TaxCode = value; OnPropertyChanged(); }
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

        public AddCompanyVM(Action<CompanyModel> onSaved, Action onCancel)
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
                ErrorMessage = "Vui lòng nhập tên công ty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TaxCode) || !Regex.IsMatch(TaxCode.Trim(), @"^\d{10,13}$"))
            {
                ErrorMessage = "Mã số thuế không hợp lệ (10-13 chữ số).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Field))
            {
                ErrorMessage = "Vui lòng nhập lĩnh vực hoạt động.";
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
        /// TODO: thay Task.Delay bằng gọi service thực tế (POST /companies) để tạo công ty,
        /// company vừa tạo sẽ ở trạng thái Pending chờ Admin duyệt (mục 4.4 - Quản lý công ty).
        /// </summary>
        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;
            await Task.Delay(400);

            var company = new CompanyModel
            {
                Id = "C" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                Name = Name.Trim(),
                TaxCode = TaxCode.Trim(),
                Field = Field.Trim(),
                Email = Email.Trim(),
                Phone = Phone?.Trim(),
                Address = Address?.Trim(),
                Package = Package,
                Status = CompanyStatus.Pending,
                JoinedDate = DateTime.Now,
                StaffCount = 0,
                ProjectCount = 0
            };

            IsBusy = false;
            _onSaved?.Invoke(company);
        }
    }
}