using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root TrainingCenter/School (mục 4.8 - Quản lý cơ sở đào tạo).</summary>
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

        private School() { }

        private School(string schoolName, string field, string email, string phone, Address address)
        {
            SchoolName = schoolName;
            Field = field;
            Email = email;
            Phone = phone;
            Address = address;
            JoinDate = DateTime.UtcNow;
            IsVerified = false;
        }

        public static Result<School> Create(string schoolName, string field, string email, string phone, Address? address = null)
        {
            if (string.IsNullOrWhiteSpace(schoolName))
                return Result<School>.Failure("Tên cơ sở đào tạo không được để trống.");
            if (string.IsNullOrWhiteSpace(field))
                return Result<School>.Failure("Lĩnh vực đào tạo không được để trống.");
            if (string.IsNullOrWhiteSpace(email))
                return Result<School>.Failure("Email không được để trống.");

            var school = new School(schoolName.Trim(), field.Trim(), email.Trim(),
                phone?.Trim() ?? string.Empty, address ?? new Address());

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

        public Result Reactivate()
        {
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