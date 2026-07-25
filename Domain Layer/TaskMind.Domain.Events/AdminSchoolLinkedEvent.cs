// AdminSchoolLinkedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi User đăng ký thành lập cơ sở đào tạo được Admin hệ thống xác minh thành công, cấp AdminSchool liên kết (mục 4.1.1, 4.8).</summary>
    public class AdminSchoolLinkedEvent : DomainEvent
    {
        public Guid AdminSchoolAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid SchoolId { get; init; }
    }
}