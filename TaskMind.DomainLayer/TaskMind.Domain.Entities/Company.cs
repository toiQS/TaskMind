using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.ObjectValues;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root Company (mục 4.4 - Quản lý công ty).</summary>
    [Index(nameof(TaxCode), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(IsVerified), nameof(Status))]
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

        /// <summary>
        /// [MỚI - fix] User gốc đã đứng ra đăng ký thành lập công ty này (mục 2.1, 4.1.1, 4.4).
        /// Trước đây trường này không tồn tại nên khi Admin duyệt (Verify()), hệ thống không có cách
        /// nào biết phải cấp tài khoản AdminCompany cho User nào — dẫn tới luồng 7.3.1 bị đứt đoạn.
        /// </summary>
        public Guid RequestedByUserId { get; private set; }

        private Company() { }

        private Company(string companyName, string taxCode, string field, string email, string phone, Address address, Guid requestedByUserId)
        {
            CompanyName = companyName;
            TaxCode = taxCode;
            Field = field;
            Email = email;
            Phone = phone;
            Address = address;
            JoinDate = DateTime.UtcNow;
            IsVerified = false;
            RequestedByUserId = requestedByUserId;
            UpdateStatus(Enums.EntityStatus.Paused);
        }

        public static Result<Company> Create(string companyName, string taxCode, string field, string email, string phone, Guid requestedByUserId, Address? address = null)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return Result<Company>.Failure("Tên công ty không được để trống.");
            if (string.IsNullOrWhiteSpace(taxCode) || taxCode.Trim().Length is < 10 or > 13)
                return Result<Company>.Failure("Mã số thuế không hợp lệ.");
            if (string.IsNullOrWhiteSpace(field))
                return Result<Company>.Failure("Lĩnh vực hoạt động không được để trống.");
            if (string.IsNullOrWhiteSpace(email))
                return Result<Company>.Failure("Email không được để trống.");
            if (string.IsNullOrWhiteSpace(phone))
                return Result<Company>.Failure("Số điện thoại không được để trống.");
            if (requestedByUserId == Guid.Empty)
                return Result<Company>.Failure("Phải xác định User đứng ra đăng ký thành lập công ty.");

            var company = new Company(companyName.Trim(), taxCode.Trim(), field?.Trim() ?? string.Empty,
                email.Trim(), phone?.Trim() ?? string.Empty, address ?? new Address(), requestedByUserId);

            return Result<Company>.Success(company);
        }

        /// <summary>Admin duyệt công ty (mục 4.4: quy trình kiểm duyệt trước khi hoạt động đầy đủ).</summary>
        public Result Verify()
        {
            if (IsVerified) return Result.Failure("Công ty đã được xác thực trước đó.");
            if (Status != Enums.EntityStatus.Paused)
                return Result.Failure("Chỉ công ty đang chờ duyệt mới có thể được xác minh.");
            IsVerified = true;
            UpdateStatus(Enums.EntityStatus.Active);
            AddDomainEvent(new CompanyVerifiedEvent { CompanyId = Id, CompanyName = CompanyName });
            return Result.Success();
        }

        public Result RejectRegistration()
        {
            if (IsVerified || Status != Enums.EntityStatus.Paused)
                return Result.Failure("Chỉ yêu cầu đăng ký đang chờ mới có thể bị từ chối.");
            UpdateStatus(Enums.EntityStatus.Deleted);
            return Result.Success();
        }

        public Result Suspend()
        {
            if (!IsVerified) return Result.Failure("Chỉ công ty đã được xác minh mới có thể tạm ngưng.");
            if (Status != Enums.EntityStatus.Active)
                return Result.Failure("Chỉ công ty đang hoạt động mới có thể tạm ngưng.");
            UpdateStatus(Enums.EntityStatus.Blocked);
            return Result.Success();
        }

        // [CẬP NHẬT - fix] Thêm guard idempotency, đối xứng với Suspend() — trước đây gọi thẳng
        // UpdateStatus(Active) bất kể trạng thái hiện tại, khiến ReactivateCompanyCommand luôn báo
        // "thành công" kể cả khi công ty vốn đã Active, không phản ánh đúng ý định của Admin.
        public Result Reactivate()
        {
            if (!IsVerified) return Result.Failure("Chỉ công ty đã được xác minh mới có thể kích hoạt.");
            if (Status == Enums.EntityStatus.Active)
                return Result.Failure("Công ty hiện đã ở trạng thái hoạt động.");
            if (Status != Enums.EntityStatus.Blocked)
                return Result.Failure("Chỉ công ty đang bị tạm ngưng mới có thể kích hoạt lại.");
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

        public Result UpdateDetails(string companyName, string field, string email, string phone, Address? address)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return Result.Failure("Tên công ty không được để trống.");
            if (string.IsNullOrWhiteSpace(field))
                return Result.Failure("Lĩnh vực hoạt động không được để trống.");
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure("Email không được để trống.");
            if (string.IsNullOrWhiteSpace(phone))
                return Result.Failure("Số điện thoại không được để trống.");

            CompanyName = companyName.Trim();
            Field = field?.Trim() ?? string.Empty;
            Email = email.Trim();
            Phone = phone?.Trim() ?? string.Empty;
            Address = address ?? new Address();
            return Result.Success();
        }

    }
}
