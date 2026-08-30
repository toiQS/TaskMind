// CertificateIssuedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi hệ thống cấp chứng chỉ điện tử cho User (mục 4.20).</summary>
    public class CertificateIssuedEvent : DomainEvent
    {
        public Guid CertificateId { get; init; }
        public Guid UserId { get; init; }
        public string VerificationCode { get; init; } = string.Empty;
    }
}
