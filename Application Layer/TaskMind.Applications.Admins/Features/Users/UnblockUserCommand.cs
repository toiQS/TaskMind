// UnblockUserCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class UnblockUserCommand : IRequest<ServiceResult>
    {
        public Guid UserId { get; }

        public UnblockUserCommand(Guid userId)
        {
            UserId = userId;
        }
    }

    public class UnblockUserHandler : IRequestHandler<UnblockUserCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public UnblockUserHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(UnblockUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user == null)
                return ServiceResult.NotFound("Không tìm thấy người dùng.");

            if (user.Status != EntityStatus.Blocked)
                return ServiceResult.Failure("Tài khoản hiện không ở trạng thái bị cấm.");

            user.UpdateStatus(EntityStatus.Active);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Mở khoá tài khoản thành công");
        }
    }
}