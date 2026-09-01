using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Staff là tài khoản phái sinh từ User (mục 2.1, 4.1.1), được cấp khi User pass phỏng vấn và
    /// xác nhận gia nhập một công ty, xác minh thành công (mục 4.5). LinkedUserId trỏ về đúng tài
    /// khoản User gốc để truy xuất thông tin cơ bản, kỹ năng và lịch sử tham gia dự án.
    ///
    /// [CẬP NHẬT - v2.1, mục 2.1.1] Mỗi lần gia nhập tạo một bản ghi Staff HOÀN TOÀN MỚI, độc lập với
    /// mọi bản ghi trước đó của cùng LinkedUserId — kể cả khi từng là Staff của chính công ty này rồi
    /// quay lại. Khi rời công ty, bản ghi chuyển IsActive = false VĨNH VIỄN (không còn Reactivate) và
    /// được giữ nguyên vẹn làm dữ liệu lịch sử/căn cứ pháp lý; LeftAtUtc cùng CreatedAtUtc (thời điểm
    /// gia nhập) xác định khoảng thời gian công tác (tenure), dùng làm căn cứ cho quy trình công ty
    /// phản ánh kỹ năng (mục 4.3.2) và lịch sử kỹ năng (mục 4.3.3). Quyền lợi của tài khoản này chỉ
    /// giới hạn trong phạm vi công ty đã cấp, không mang theo sang bản ghi Staff khác (nếu có).
    ///
    /// Ràng buộc nghiệp vụ "tối đa một bản ghi Active cho mỗi LinkedUserId tại một thời điểm" (mục
    /// 2.1.1, mục 8) cần được áp dụng bằng filtered unique index (WHERE IsActive = true) ở tầng
    /// Infrastructure/Fluent API — Index attribute dưới đây chỉ hỗ trợ tra cứu, KHÔNG còn là ràng buộc
    /// unique tuyệt đối như thiết kế ban đầu, vì nhiều bản ghi lịch sử (IsActive = false) là hợp lệ.
    /// </summary>
    [Index(nameof(CompanyId), nameof(IsActive))]
    [Index(nameof(LinkedUserId), nameof(IsActive))]
    public class Staff : Account
    {
        public Guid LinkedUserId { get; private set; }
        public Guid CompanyId { get; private set; }
        public virtual Company Company { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        /// <summary>Thời điểm rời công ty — đóng băng vĩnh viễn bản ghi này (mục 2.1.1). [MỚI - v2.1]</summary>
        public DateTimeOffset? LeftAtUtc { get; private set; }

        private Staff() : base() { }

        private Staff(Guid linkedUserId, Guid companyId)
        {
            LinkedUserId = linkedUserId;
            CompanyId = companyId;
        }

        /// <summary>
        /// Cấp một bản ghi Staff MỚI cho một lượt gia nhập công ty (mục 2.1.1) — luôn tạo mới, kể cả
        /// khi LinkedUserId đã từng có bản ghi Staff (đang Active hoặc đã ngừng hoạt động) trước đó.
        /// </summary>
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

        /// <summary>
        /// Rời công ty: đóng băng bản ghi VĨNH VIỄN (mục 2.1.1). KHÔNG có Reactivate() — nếu nhân sự
        /// quay lại công ty này (hoặc công ty khác) sau này, hệ thống phải cấp một bản ghi Staff MỚI
        /// hoàn toàn qua Staff.Create, không tái sử dụng bản ghi cũ.
        /// </summary>
        public Result Deactivate()
        {
            if (!IsActive) return Result.Failure("Nhân sự đã ở trạng thái ngừng hoạt động.");

            IsActive = false;
            LeftAtUtc = DateTimeOffset.UtcNow;

            AddDomainEvent(new StaffLeftEvent
            {
                StaffAccountId = Id,
                LinkedUserId = LinkedUserId,
                CompanyId = CompanyId,
                JoinedAtUtc = CreatedAtUtc,
                LeftAtUtc = LeftAtUtc.Value
            });

            return Result.Success();
        }
    }
}
