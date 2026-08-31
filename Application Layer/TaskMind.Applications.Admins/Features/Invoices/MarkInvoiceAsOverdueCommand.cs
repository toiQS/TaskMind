using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Invoices
{
    public class MarkInvoiceAsOverdueCommand : ServiceResult
    {
        public Guid InvoiceId { get; }

        public MarkInvoiceAsOverdueCommand(Guid invoiceId)
        {
            InvoiceId = invoiceId;
        }
    }

    public class MarkInvoiceAsOverdueHandler
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Đánh dấu hoá đơn quá hạn thành công");
        }
    }
}