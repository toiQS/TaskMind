using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Profit
{
    /// <summary>Xuất hoá đơn cho một giao dịch đang ở trạng thái Chờ xuất HĐ (mục 5.5).</summary>
    public class IssueInvoiceCommand : IRequest<InvoiceDto>
    {
        public Guid InvoiceId { get; set; }
    }

    public class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, InvoiceDto>
    {
        private readonly IApplicationDbContext _db;

        public IssueInvoiceCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<InvoiceDto> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

            var result = invoice.MarkAsIssued();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return InvoiceMapper.ToDto(invoice);
        }
    }
}
