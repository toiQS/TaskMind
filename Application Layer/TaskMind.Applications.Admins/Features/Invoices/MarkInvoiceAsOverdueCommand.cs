// MarkInvoiceAsOverdueCommand.cs
// [CẬP NHẬT - fix] Trước đây không có ApproverAdminId/AuditLog/Notification, trong khi
// MarkInvoiceAsPaidCommand (thao tác đối xứng) đã có cả hai. Đánh dấu quá hạn ảnh hưởng trực tiếp tới
// đối tác (Company/School/ExchangeContract) nên cần thông báo, dùng lại logic tra cứu người nhận
// tương tự InvoiceIssuedEventHandler/InvoicePaidEventHandler.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

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

            Guid? recipientAccountId = invoice.SourceType switch
            {
                InvoiceSourceType.CompanySubscription => (await _dbContext.AdminCompanies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ac => ac.CompanyId == invoice.SourceRefId, cancellationToken))?.LinkedUserId,

                InvoiceSourceType.SchoolSubscription => (await _dbContext.AdminSchools
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.SchoolId == invoice.SourceRefId, cancellationToken))?.LinkedUserId,

                InvoiceSourceType.ExchangeFee => (await _dbContext.ExchangeContracts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == invoice.SourceRefId, cancellationToken))?.PartyAAccountId,

                _ => null
            };

            if (recipientAccountId is not null && recipientAccountId != Guid.Empty)
            {
                var notifResult = Notification.Create(
                    recipientAccountId.Value,
                    "Hoá đơn quá hạn thanh toán",
                    $"Hoá đơn trị giá {invoice.Amount.Amount:N0} {invoice.Amount.Currency} của bạn đã quá hạn thanh toán. Vui lòng thanh toán sớm nhất có thể.",
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Đánh dấu hoá đơn quá hạn thành công");
        }
    }
}
