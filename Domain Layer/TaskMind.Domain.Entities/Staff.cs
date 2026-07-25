using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Staff là tài khoản phái sinh từ User (mục 2.1, 4.1.1), được cấp khi User được một Company
    /// mời và xác minh thành công (mục 4.5). LinkedUserId trỏ về đúng tài khoản User gốc để truy xuất
    /// thông tin cơ bản, kỹ năng và lịch sử tham gia dự án.
    /// </summary>
    public class Staff : Account
    {
        public Guid LinkedUserId { get; private set; }
        public Guid CompanyId { get; private set; }
        public virtual Company Company { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        private Staff() : base() { }

        private Staff(Guid linkedUserId, Guid companyId)
        {
            LinkedUserId = linkedUserId;
            CompanyId = companyId;
        }

        public static Result<Staff> Create(
            string citizenId,
            string email,
            string passwordHash,
            Guid linkedUserId,
            Guid companyId)
        {
            if (linkedUserId == Guid.Empty)
                return Result<Staff>.Failure("LinkedUserId không hợp lệ.");
            if (companyId == Guid.Empty)
                return Result<Staff>.Failure("CompanyId không hợp lệ.");

            var staff = new Staff(linkedUserId, companyId);
            var result = staff.InitializeWithCredentials(citizenId, email, AccountRole.Staff, passwordHash);
            if (!result.IsSuccess)
                return Result<Staff>.Failure(result.Message);

            staff.AddDomainEvent(new StaffJoinedEvent
            {
                StaffAccountId = staff.Id,
                LinkedUserId = linkedUserId,
                CompanyId = companyId
            });

            return Result<Staff>.Success(staff);
        }

        public Result Deactivate()
        {
            if (!IsActive) return Result.Failure("Nhân sự đã ở trạng thái ngừng hoạt động.");
            IsActive = false;
            return Result.Success();
        }

        public Result Reactivate()
        {
            if (IsActive) return Result.Failure("Nhân sự đang hoạt động.");
            IsActive = true;
            return Result.Success();
        }
    }
}