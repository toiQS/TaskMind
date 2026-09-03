// IssueSchoolSubscriptionInvoiceCommand.cs — [MỚI - fix] Tương tự IssueCompanySubscriptionInvoiceCommand,
// cho nguồn thu 3 (SchoolSubscription, mục 4.14).
//
// [CẬP NHẬT - fix] Bổ sung ApproverAdminId + AuditLog, cùng lý do với IssueCompanySubscriptionInvoiceCommand.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Invoices
{
    public class IssueSchoolSubscriptionInvoiceCommand : IRequest<ServiceResult<Guid>>
    {
        public Guid SchoolId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public Guid ApproverAdminId { get; }

        public IssueSchoolSubscriptionInvoiceCommand(Guid schoolId, decimal amount, Guid approverAdminId, string currency = "VND")
        {
            SchoolId = schoolId;
            Amount = amount;
            Currency = currency;
            ApproverAdminId = approverAdminId;
        }
    }

    public class IssueSchoolSubscriptionInvoiceHandler : IRequestHandler<IssueSchoolSubscriptionInvoiceCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public IssueSchoolSubscriptionInvoiceHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(IssueSchoolSubscriptionInvoiceCommand command, CancellationToken cancellationToken)
        {
            var schoolExists = await _dbContext.Schools.AsNoTracking()
                .AnyAsync(s => s.Id == command.SchoolId, cancellationToken);
            if (!schoolExists)
                return ServiceResult<Guid>.NotFound("Không tìm thấy cơ sở đào tạo.");

            var amount = Money.Of(command.Amount, command.Currency);
            var invoiceResult = Invoice.Create(InvoiceSourceType.SchoolSubscription, command.SchoolId, amount);
            if (!invoiceResult.IsSuccess)
                return ServiceResult<Guid>.Failure(invoiceResult.Message);

            _dbContext.Invoices.Add(invoiceResult.Data!);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SchoolSubscriptionInvoiceIssued", nameof(Invoice), invoiceResult.Data!.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Success(invoiceResult.Data!.Id, "Sinh hoá đơn phí tham gia cơ sở đào tạo thành công");
        }
    }
}