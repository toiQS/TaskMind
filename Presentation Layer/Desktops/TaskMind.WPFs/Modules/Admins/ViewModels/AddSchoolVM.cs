using MediatR;
using System.Text.RegularExpressions;
using System.Windows.Input;
using TaskMind.Applications.Admins.Features.Schools;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class AddSchoolVM : ViewModelBase
    {
        private readonly Action<SchoolModel> _onSaved;
        private readonly Action _onCancel;
        private readonly IMediator _mediator;

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

        public AddSchoolVM(Action<SchoolModel> onSaved, Action onCancel, IMediator mediator)
        {
            _onSaved = onSaved;
            _onCancel = onCancel;
            _mediator = MediatorResolver.Resolve(mediator);

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

        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;

            var dto = await _mediator.Send(new CreateSchoolCommand
            {
                Name = Name.Trim(),
                Field = Field.Trim(),
                Email = Email.Trim(),
                Phone = Phone?.Trim(),
                Street = Address?.Trim(),
                Package = Package
            });

            IsBusy = false;
            _onSaved?.Invoke(SchoolUiMapper.ToUi(dto));
        }
    }
}