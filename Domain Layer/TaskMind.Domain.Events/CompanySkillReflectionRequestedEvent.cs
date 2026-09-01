// CompanySkillReflectionRequestedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi công ty khởi tạo đề xuất phản ánh kỹ năng cho một Staff (mục 4.3.2). [MỚI - v2.1]</summary>
    public class CompanySkillReflectionRequestedEvent : DomainEvent
    {
        public Guid RequestId { get; init; }
        public Guid CompanyId { get; init; }
        public Guid UserId { get; init; }
        public Guid StaffAccountId { get; init; }
        public Guid SkillId { get; init; }
        public SkillReflectionType ReflectionType { get; init; }
        public Guid ResponsibleAccountId { get; init; }

        /// <summary>true nếu là Down — cần Admin hệ thống xem xét trước khi tổ chức xác minh lại (mục 4.3.2).</summary>
        public bool RequiresAdminReview { get; init; }
    }
}
