using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    /// <summary>Admin khoá (cấm) một tài khoản User do vi phạm (mục 4.1: quản lý trạng thái tài khoản).</summary>
    public class BlockUserCommand : ServiceResult
    {
        public Guid UserId { get; }
        public string? Reason { get; }

        public BlockUserCommand(Guid userId, string? reason = null)
        {
            UserId = userId;
            Reason = reason;
        }
    }

    public class BlockUserHandler
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

            // TODO: AuditLog.Record(adminId, "UserBlocked", nameof(User), user.Id)

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Khoá tài khoản thành công");
        }
    }
}