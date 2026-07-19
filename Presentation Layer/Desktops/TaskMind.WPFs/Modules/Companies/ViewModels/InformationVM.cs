using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class InformationVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

        private CompanyInfoModel _companyInfo;
        public CompanyInfoModel CompanyInfo
        {
            get => _companyInfo;
            set { _companyInfo = value; OnPropertyChanged(); }
        }

        /// <summary>Bản sao dùng khi Huỷ chỉnh sửa, khôi phục lại dữ liệu gốc.</summary>
        private CompanyInfoModel _backup;

        private MembershipPackageModel _membership;
        public MembershipPackageModel Membership
        {
            get => _membership;
            set { _membership = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        public ICommand RefreshCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RenewMembershipCommand { get; }

        public InformationVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            EditCommand = new RelayCommand(_ => StartEdit());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            RenewMembershipCommand = new RelayCommand(async _ => await RenewMembershipAsync());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/information thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            CompanyInfo = new CompanyInfoModel
            {
                CompanyName = "TaskMind Software JSC",
                TaxCode = "0312345678",
                Industry = "Phát triển phần mềm",
                CompanySize = "10 - 50 nhân viên",
                FoundedDate = new DateTime(2019, 3, 1),
                Website = "https://taskmind.vn",
                Description = "Công ty chuyên phát triển phần mềm quản lý dự án và nền tảng đào tạo cho doanh nghiệp vừa và nhỏ.",
                Address = "123 Nguyễn Văn Linh, Quận 7, TP. Hồ Chí Minh",
                Phone = "028 1234 5678",
                Email = "contact@taskmind.vn",
                LegalRepresentativeName = "Trần Văn Bình",
                LegalRepresentativePosition = "Giám đốc điều hành",
                VerificationStatus = CompanyVerificationStatus.Verified,
                SubmittedDate = DateTime.Now.AddMonths(-6),
                VerifiedDate = DateTime.Now.AddMonths(-6).AddDays(3)
            };

            Membership = new MembershipPackageModel
            {
                PlanName = "Gói Doanh nghiệp",
                Price = 2_500_000m,
                BillingCycle = BillingCycle.Monthly,
                ExpiryDate = DateTime.Now.AddDays(12),
                IsAutoRenew = true
            };

            IsBusy = false;
        }

        private void StartEdit()
        {
            ErrorMessage = string.Empty;

            // Lưu bản sao để có thể khôi phục nếu người dùng Huỷ
            _backup = new CompanyInfoModel
            {
                Id = CompanyInfo.Id,
                CompanyName = CompanyInfo.CompanyName,
                TaxCode = CompanyInfo.TaxCode,
                Industry = CompanyInfo.Industry,
                CompanySize = CompanyInfo.CompanySize,
                FoundedDate = CompanyInfo.FoundedDate,
                Website = CompanyInfo.Website,
                Description = CompanyInfo.Description,
                Address = CompanyInfo.Address,
                Phone = CompanyInfo.Phone,
                Email = CompanyInfo.Email,
                LegalRepresentativeName = CompanyInfo.LegalRepresentativeName,
                LegalRepresentativePosition = CompanyInfo.LegalRepresentativePosition,
                VerificationStatus = CompanyInfo.VerificationStatus,
                VerificationNote = CompanyInfo.VerificationNote,
                SubmittedDate = CompanyInfo.SubmittedDate,
                VerifiedDate = CompanyInfo.VerifiedDate
            };

            IsEditing = true;
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CompanyInfo.CompanyName))
            {
                ErrorMessage = "Tên công ty không được để trống.";
                return;
            }

            if (string.IsNullOrWhiteSpace(CompanyInfo.TaxCode))
            {
                ErrorMessage = "Vui lòng nhập mã số thuế.";
                return;
            }

            IsBusy = true;

            // TODO: gọi service PUT /company/{companyId}/information
            // Sửa thông tin sau khi đã Verified có thể cần đưa về PendingVerification chờ duyệt lại tuỳ chính sách hệ thống.
            await Task.Delay(500);

            IsBusy = false;
            IsEditing = false;
        }

        private void CancelEdit()
        {
            if (_backup != null)
                CompanyInfo = _backup;

            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        private async Task RenewMembershipAsync()
        {
            if (Membership == null) return;

            IsBusy = true;

            // TODO: gọi service POST /company/{companyId}/membership/renew (liên kết mục 5.5 - Payment/Invoice)
            await Task.Delay(500);

            Membership.ExpiryDate = Membership.BillingCycle == BillingCycle.Monthly
                ? Membership.ExpiryDate.AddMonths(1)
                : Membership.ExpiryDate.AddYears(1);

            // Ép làm mới UI vì MembershipPackageModel không implement INotifyPropertyChanged
            var updated = Membership;
            Membership = null;
            Membership = updated;

            IsBusy = false;
        }
    }
}