// ExchangeContractCompletedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một ExchangeContract hoàn tất; kích hoạt tạo Invoice khấu trừ phí dịch vụ (mục 4.13, 4.14).</summary>
    public class ExchangeContractCompletedEvent : DomainEvent
    {
        public Guid ExchangeContractId { get; init; }
        public Guid ProjectId { get; init; }
        public decimal ServiceFeeAmount { get; init; }
        public string Currency { get; init; } = "VND";
    }
}