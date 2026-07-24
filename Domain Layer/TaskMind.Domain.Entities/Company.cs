using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root Company (mục 4.4 - Quản lý công ty).</summary>
    public class Company : AuditableAggregateRoot
    {
        public string CompanyName { get; private set; } = string.Empty;
        public string TaxCode { get; private set; } = string.Empty;
        public string Field { get; private set; } = string.Empty;
        public Address Address { get; private set; } = new Address();
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public DateTime JoinDate { get; private set; }

        /// <summary>Trạng thái kiểm duyệt: đang dùng chung EntityStatus (Active/Paused/Blocked...) của EntityBase,
        /// việc chờ duyệt được coi là Paused cho tới khi Admin xác thực (mục 4.4).</summary>
        public bool IsVerified { get; private set; }

        /// <summary>Mã gói tham gia hệ thống, liên kết mục 4.13 - Quản lý lợi nhuận (Nguồn thu 2).</summary>
        public string MembershipPackage { get; private set; } = "Starter";

        private Company() { }

        private Company(string companyName, string taxCode, string field, string email, string phone, Address address)
        {
            CompanyName = companyName;
            TaxCode = taxCode;
            Field = field;
            Email = email;
            Phone = phone;
            Address = address;
            JoinDate = DateTime.UtcNow;
            IsVerified = false;
        }

        public static Result<Company> Create(string companyName, string taxCode, string field, string email, string phone, Address? address = null)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return Result<Company>.Failure("Tên công ty không được để trống.");
            if (string.IsNullOrWhiteSpace(taxCode) || taxCode.Trim().Length is < 10 or > 13)
                return Result<Company>.Failure("Mã số thuế không hợp lệ.");
            if (string.IsNullOrWhiteSpace(email))
                return Result<Company>.Failure("Email không được để trống.");

            var company = new Company(companyName.Trim(), taxCode.Trim(), field?.Trim() ?? string.Empty,
                email.Trim(), phone?.Trim() ?? string.Empty, address ?? new Address());

            return Result<Company>.Success(company);
        }

        /// <summary>Admin duyệt công ty (mục 4.4: quy trình kiểm duyệt trước khi hoạt động đầy đủ).</summary>
        public Result Verify()
        {
            if (IsVerified) return Result.Failure("Công ty đã được xác thực trước đó.");
            IsVerified = true;
            UpdateStatus(Enums.EntityStatus.Active);
            return Result.Success();
        }

        public Result Suspend()
        {
            if (Status == Enums.EntityStatus.Blocked) return Result.Failure("Công ty đã tạm ngưng.");
            UpdateStatus(Enums.EntityStatus.Blocked);
            return Result.Success();
        }

        public Result Reactivate()
        {
            UpdateStatus(Enums.EntityStatus.Active);
            return Result.Success();
        }

        public Result ChangeMembershipPackage(string package)
        {
            if (string.IsNullOrWhiteSpace(package))
                return Result.Failure("Gói dịch vụ không hợp lệ.");
            MembershipPackage = package;
            return Result.Success();
        }

        public Result UpdateContactInfo(string email, string phone, Address? address)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure("Email không được để trống.");
            Email = email.Trim();
            Phone = phone?.Trim() ?? Phone;
            if (address != null) Address = address;
            return Result.Success();
        }
    }
}