// AdminCompanyLinkedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi User đăng ký thành lập công ty được Admin hệ thống xác minh thành công, cấp AdminCompany liên kết (mục 4.1.1, 4.4).</summary>
    public class AdminCompanyLinkedEvent : DomainEvent
    {
        public Guid AdminCompanyAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid CompanyId { get; init; }
    }
}