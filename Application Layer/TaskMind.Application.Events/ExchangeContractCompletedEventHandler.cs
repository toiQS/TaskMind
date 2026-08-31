using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một ExchangeContract hoàn tất (mục 4.13, 4.14, 7.3.6): tạo Invoice khấu trừ phí dịch vụ
    /// với SourceType = ExchangeFee, SourceRefId = ExchangeContractId.
    /// </summary>
    public class ExchangeContractCompletedEventHandler : INotificationHandler<ExchangeContractCompletedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public ExchangeContractCompletedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(ExchangeContractCompletedEvent notification, CancellationToken cancellationToken)
        {
            var amount = Money.Of(notification.ServiceFeeAmount, notification.Currency);

            var invoiceResult = Invoice.Create(InvoiceSourceType.ExchangeFee, notification.ExchangeContractId, amount);
            if (invoiceResult.IsSuccess)
                _dbContext.Invoices.Add(invoiceResult.Data!);

            // invoiceResult thất bại khi ServiceFeeAmount = 0 (miễn phí dịch vụ) — không phát sinh hoá đơn, không phải lỗi.

            return Task.CompletedTask;
        }
    }
}
