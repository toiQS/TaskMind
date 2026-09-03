// MarkInvoiceAsOverdueCommand.cs
// [CẬP NHẬT - fix] Trước đây handler tự copy-paste toàn bộ logic "tra recipientAccountId theo
// SourceType" (đã lặp lại ở InvoiceIssuedEventHandler và InvoicePaidEventHandler) — rủi ro quên cập
// nhật một trong ba nơi khi có InvoiceSourceType mới. Giờ Invoice.MarkAsOverdue() tự phát sinh
// InvoiceOverdueEvent, InvoiceOverdueEventHandler lo toàn bộ phần Notification, handler này chỉ còn
// tập trung vào state transition + AuditLog.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Invoices
{
    public class MarkInvoiceAsOverdueCommand : IRequest<ServiceResult>
    {
        public Guid InvoiceId { get; }
        public Guid ApproverAdminId { get; }

        public MarkInvoiceAsOverdueCommand(Guid invoiceId, Guid approverAdminId)
        {
            InvoiceId = invoiceId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class MarkInvoiceAsOverdueHandler : IRequestHandler<MarkInvoiceAsOverdueCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public MarkInvoiceAsOverdueHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(MarkInvoiceAsOverdueCommand command, CancellationToken cancellationToken)
        {
            var invoice = await _dbContext.Invoices
                .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken);

            if (invoice == null)
                return ServiceResult.NotFound("Không tìm thấy hoá đơn.");

            var result = invoice.MarkAsOverdue();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "InvoiceMarkedOverdue", nameof(Invoice), invoice.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            // Invoice.MarkAsOverdue() đã tự phát sinh InvoiceOverdueEvent -> InvoiceOverdueEventHandler
            // lo phần Notification, nhất quán với luồng MarkAsPaid()/InvoicePaidEventHandler.
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Đánh dấu hoá đơn quá hạn thành công");
        }
    }
}