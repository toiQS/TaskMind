// MarkInvoiceAsPaidCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Invoices
{
    public class MarkInvoiceAsPaidCommand : IRequest<ServiceResult>
    {
        public Guid InvoiceId { get; }
        public Guid ApproverAdminId { get; }

        public MarkInvoiceAsPaidCommand(Guid invoiceId, Guid approverAdminId)
        {
            InvoiceId = invoiceId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class MarkInvoiceAsPaidHandler : IRequestHandler<MarkInvoiceAsPaidCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public MarkInvoiceAsPaidHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(MarkInvoiceAsPaidCommand command, CancellationToken cancellationToken)
        {
            var invoice = await _dbContext.Invoices
                .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken);

            if (invoice == null)
                return ServiceResult.NotFound("Không tìm thấy hoá đơn.");

            var result = invoice.MarkAsPaid();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "PaymentIssued", nameof(Invoice), invoice.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Đánh dấu hoá đơn đã thanh toán thành công");
        }
    }
}