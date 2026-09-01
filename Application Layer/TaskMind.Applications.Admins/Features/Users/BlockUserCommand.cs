// BlockUserCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class BlockUserCommand : IRequest<ServiceResult>
    {
        public Guid UserId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public BlockUserCommand(Guid userId, Guid approverAdminId, string? reason = null)
        {
            UserId = userId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class BlockUserHandler : IRequestHandler<BlockUserCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public BlockUserHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(BlockUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user == null)
                return ServiceResult.NotFound("Không tìm thấy người dùng.");

            if (user.Status == EntityStatus.Blocked)
                return ServiceResult.Failure("Tài khoản đã bị cấm trước đó.");

            user.UpdateStatus(EntityStatus.Blocked);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "UserBlocked", nameof(User), user.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Khoá tài khoản thành công");
        }
    }
}