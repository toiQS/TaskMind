using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    public enum InvoicePartnerType { Company, School }

    /// <summary>Hoá đơn cho phí tham gia hệ thống hoặc phí giao dịch trao đổi (mục 4.13, 5.5).</summary>
    public class Invoice : AuditableAggregateRoot
    {
        public Guid PartnerId { get; private set; }
        public InvoicePartnerType PartnerType { get; private set; }
        public Money Amount { get; private set; } = Money.Of(0);
        public InvoiceStatus InvoiceStatus { get; private set; } = InvoiceStatus.Pending;
        public Guid? RelatedExchangeContractId { get; private set; }
        public DateTime? PaidAtUtc { get; private set; }

        private Invoice() { }

        private Invoice(Guid partnerId, InvoicePartnerType partnerType, Money amount, Guid? relatedExchangeContractId)
        {
            PartnerId = partnerId;
            PartnerType = partnerType;
            Amount = amount;
            RelatedExchangeContractId = relatedExchangeContractId;
        }

        public static Result<Invoice> Create(Guid partnerId, InvoicePartnerType partnerType, Money amount, Guid? relatedExchangeContractId = null)
        {
            if (partnerId == Guid.Empty)
                return Result<Invoice>.Failure("PartnerId không hợp lệ.");
            if (amount.Amount <= 0)
                return Result<Invoice>.Failure("Số tiền hoá đơn phải lớn hơn 0.");

            var invoice = new Invoice(partnerId, partnerType, amount, relatedExchangeContractId);
            invoice.AddDomainEvent(new InvoiceIssuedEvent
            {
                InvoiceId = invoice.Id,
                PartnerId = partnerId,
                Amount = amount.Amount,
                Currency = amount.Currency
            });
            return Result<Invoice>.Success(invoice);
        }

        public Result MarkAsIssued()
        {
            if (InvoiceStatus != InvoiceStatus.Pending)
                return Result.Failure("Chỉ hoá đơn đang chờ mới có thể chuyển sang đã xuất.");
            InvoiceStatus = InvoiceStatus.Issued;
            return Result.Success();
        }

        public Result MarkAsPaid()
        {
            if (InvoiceStatus == InvoiceStatus.Paid)
                return Result.Failure("Hoá đơn đã được thanh toán trước đó.");

            InvoiceStatus = InvoiceStatus.Paid;
            PaidAtUtc = DateTime.UtcNow;

            AddDomainEvent(new InvoicePaidEvent { InvoiceId = Id, PartnerId = PartnerId, Amount = Amount.Amount });
            return Result.Success();
        }

        public Result MarkAsOverdue()
        {
            if (InvoiceStatus == InvoiceStatus.Paid)
                return Result.Failure("Hoá đơn đã thanh toán, không thể đánh dấu quá hạn.");
            InvoiceStatus = InvoiceStatus.Overdue;
            return Result.Success();
        }
    }
}