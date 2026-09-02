// DeactivateStaffCommand.cs — [MỚI - fix] Domain đã có Staff.Deactivate() phát sinh StaffLeftEvent
// (mục 2.1.1) nhưng trước đây không có command nào ở tầng Application gọi tới nó — nghĩa là không ai
// có thể đánh dấu một nhân sự đã rời công ty qua API/luồng nghiệp vụ.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class DeactivateStaffCommand : IRequest<ServiceResult>
    {
        public Guid StaffAccountId { get; }
        public Guid ApproverAdminId { get; }

        public DeactivateStaffCommand(Guid staffAccountId, Guid approverAdminId)
        {
            StaffAccountId = staffAccountId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class DeactivateStaffHandler : IRequestHandler<DeactivateStaffCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeactivateStaffHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(DeactivateStaffCommand command, CancellationToken cancellationToken)
        {
            var staff = await _dbContext.Staffs
                .FirstOrDefaultAsync(s => s.Id == command.StaffAccountId, cancellationToken);

            if (staff == null)
                return ServiceResult.NotFound("Không tìm thấy nhân sự.");

            var result = staff.Deactivate();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "StaffLeftCompany", nameof(Staff), staff.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Ghi nhận nhân sự rời công ty thành công");
        }
    }
}
