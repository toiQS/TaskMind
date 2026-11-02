// CompanyVerifiedEvent.cs
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi Admin hệ thống duyệt một công ty đăng ký (mục 4.4), công ty chuyển sang hoạt động đầy đủ.</summary>
    public class CompanyVerifiedEvent : DomainEvent
    {
        public Guid CompanyId { get; init; }
        public string CompanyName { get; init; } = string.Empty;
    }
}