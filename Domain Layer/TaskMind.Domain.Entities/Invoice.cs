using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// [CẬP NHẬT] Hoá đơn cho phí tham gia hệ thống hoặc phí giao dịch trao đổi (mục 4.13, 4.14).
    /// Thay PartnerId/PartnerType (enum InvoicePartnerType cũ) bằng SourceRefId/SourceType
    /// (Enums.InvoiceSourceType: ExchangeFee/CompanySubscription/SchoolSubscription) để khớp tài liệu v2.
    /// SourceRefId tham chiếu đa hình tới ExchangeContract.Id / Company.Id / School.Id tuỳ SourceType
    /// (mục 8 - vấn đề mở: không thể dùng FK thông thường cho tham chiếu đa hình này).
    /// </summary>
    [Index(nameof(SourceRefId), nameof(InvoiceStatus))]
    [Index(nameof(SourceType))]
    public class Invoice : AuditableAggregateRoot
    {
        public InvoiceSourceType SourceType { get; private set; }

        /// <summary>ExchangeContract.Id nếu SourceType = ExchangeFee; Company.Id nếu CompanySubscription; School.Id nếu SchoolSubscription.</summary>
        public Guid SourceRefId { get; private set; }
        public Money Amount { get; private set; } = Money.Of(0);
        public InvoiceStatus InvoiceStatus { get; private set; } = InvoiceStatus.Pending;
        public DateTime? PaidAtUtc { get; private set; }

        private Invoice() { }

        private Invoice(InvoiceSourceType sourceType, Guid sourceRefId, Money amount)
        {
            SourceType = sourceType;
            SourceRefId = sourceRefId;
            Amount = amount;
        }

        public static Result<Invoice> Create(InvoiceSourceType sourceType, Guid sourceRefId, Money amount)
        {
            if (sourceRefId == Guid.Empty)
                return Result<Invoice>.Failure("SourceRefId không hợp lệ.");
            if (amount.Amount <= 0)
                return Result<Invoice>.Failure("Số tiền hoá đơn phải lớn hơn 0.");

            var invoice = new Invoice(sourceType, sourceRefId, amount);
            invoice.AddDomainEvent(new InvoiceIssuedEvent
            {
                InvoiceId = invoice.Id,
                SourceType = sourceType,
                SourceRefId = sourceRefId,
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

            AddDomainEvent(new InvoicePaidEvent
            {
                InvoiceId = Id,
                SourceType = SourceType,      
                SourceRefId = SourceRefId,
                Amount = Amount.Amount
            });
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
