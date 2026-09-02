// IssueCompanySubscriptionInvoiceCommand.cs — [MỚI - fix] Nguồn thu 2 (CompanySubscription, mục 4.14)
// trước đây hoàn toàn không có cách nào phát sinh Invoice — chỉ có ExchangeFee được tự động sinh qua
// ExchangeContractCompletedEventHandler. Command này cho Admin (hoặc một scheduled job sau này —
// xem mục 5.1) chủ động sinh hoá đơn phí tham gia định kỳ cho công ty.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Invoices
{
    public class IssueCompanySubscriptionInvoiceCommand : IRequest<ServiceResult<Guid>>
    {
        public Guid CompanyId { get; }
        public decimal Amount { get; }
        public string Currency { get; }

        public IssueCompanySubscriptionInvoiceCommand(Guid companyId, decimal amount, string currency = "VND")
        {
            CompanyId = companyId;
            Amount = amount;
            Currency = currency;
        }
    }

    public class IssueCompanySubscriptionInvoiceHandler : IRequestHandler<IssueCompanySubscriptionInvoiceCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public IssueCompanySubscriptionInvoiceHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(IssueCompanySubscriptionInvoiceCommand command, CancellationToken cancellationToken)
        {
            var companyExists = await _dbContext.Companies.AsNoTracking()
                .AnyAsync(c => c.Id == command.CompanyId, cancellationToken);
            if (!companyExists)
                return ServiceResult<Guid>.NotFound("Không tìm thấy công ty.");

            var amount = Money.Of(command.Amount, command.Currency);
            var invoiceResult = Invoice.Create(InvoiceSourceType.CompanySubscription, command.CompanyId, amount);
            if (!invoiceResult.IsSuccess)
                return ServiceResult<Guid>.Failure(invoiceResult.Message);

            _dbContext.Invoices.Add(invoiceResult.Data!);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Invoice.Create() đã tự phát sinh InvoiceIssuedEvent → InvoiceIssuedEventHandler lo phần Notification.
            return ServiceResult<Guid>.Success(invoiceResult.Data!.Id, "Sinh hoá đơn phí tham gia công ty thành công");
        }
    }
}
