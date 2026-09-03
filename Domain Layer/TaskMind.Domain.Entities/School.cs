using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root TrainingCenter/School (mục 4.8 - Quản lý cơ sở đào tạo).</summary>
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(IsVerified), nameof(Status))]
    public class School : AuditableAggregateRoot
    {
        public string SchoolName { get; private set; } = string.Empty;
        public string Field { get; private set; } = string.Empty;
        public Address Address { get; private set; } = new Address();
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public DateTime JoinDate { get; private set; }
        public bool IsVerified { get; private set; }
        public string MembershipPackage { get; private set; } = "Starter";

        /// <summary>
        /// [MỚI - fix] User gốc đã đứng ra đăng ký thành lập cơ sở đào tạo này (mục 2.1, 4.1.1, 4.8).
        /// Trước đây trường này không tồn tại nên khi Admin duyệt (Verify()), hệ thống không có cách
        /// nào biết phải cấp tài khoản AdminSchool cho User nào — dẫn tới luồng 7.3.1 bị đứt đoạn.
        /// </summary>
        public Guid RequestedByUserId { get; private set; }

        private School() { }

        private School(string schoolName, string field, string email, string phone, Address address, Guid requestedByUserId)
        {
            SchoolName = schoolName;
            Field = field;
            Email = email;
            Phone = phone;
            Address = address;
            JoinDate = DateTime.UtcNow;
            IsVerified = false;
            RequestedByUserId = requestedByUserId;
        }

        public static Result<School> Create(string schoolName, string field, string email, string phone, Guid requestedByUserId, Address? address = null)
        {
            if (string.IsNullOrWhiteSpace(schoolName))
                return Result<School>.Failure("Tên cơ sở đào tạo không được để trống.");
            if (string.IsNullOrWhiteSpace(field))
                return Result<School>.Failure("Lĩnh vực đào tạo không được để trống.");
            if (string.IsNullOrWhiteSpace(email))
                return Result<School>.Failure("Email không được để trống.");
            if (requestedByUserId == Guid.Empty)
                return Result<School>.Failure("Phải xác định User đứng ra đăng ký thành lập cơ sở đào tạo.");

            var school = new School(schoolName.Trim(), field.Trim(), email.Trim(),
                phone?.Trim() ?? string.Empty, address ?? new Address(), requestedByUserId);

            return Result<School>.Success(school);
        }

        public Result Verify()
        {
            if (IsVerified) return Result.Failure("Cơ sở đào tạo đã được xác thực trước đó.");
            IsVerified = true;
            UpdateStatus(EntityStatus.Active);
            AddDomainEvent(new SchoolVerifiedEvent { SchoolId = Id, SchoolName = SchoolName });
            return Result.Success();
        }

        public Result Suspend()
        {
            UpdateStatus(EntityStatus.Blocked);
            return Result.Success();
        }

        // [CẬP NHẬT - fix] Thêm guard idempotency, đối xứng với Suspend() — cùng lý do như Company.Reactivate().
        public Result Reactivate()
        {
            if (Status == EntityStatus.Active)
                return Result.Failure("Cơ sở đào tạo hiện đã ở trạng thái hoạt động.");
            UpdateStatus(EntityStatus.Active);
            return Result.Success();
        }

        public Result ChangeMembershipPackage(string package)
        {
            if (string.IsNullOrWhiteSpace(package))
                return Result.Failure("Gói dịch vụ không hợp lệ.");
            MembershipPackage = package;
            return Result.Success();
        }
    }
}
