// SchoolVerifiedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi Admin hệ thống duyệt một cơ sở đào tạo đăng ký (mục 4.8).</summary>
    public class SchoolVerifiedEvent : DomainEvent
    {
        public Guid SchoolId { get; init; }
        public string SchoolName { get; init; } = string.Empty;
    }
}