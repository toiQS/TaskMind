using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class ToggleLockUserCommand : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
    }

    public class ToggleLockUserCommandHandler : IRequestHandler<ToggleLockUserCommand, UserDto>
    {
        private readonly IApplicationDbContext _db;

        public ToggleLockUserCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserDto> Handle(ToggleLockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

            if (user.Status == EntityStatus.Blocked)
                throw new InvalidOperationException("Tài khoản đang bị cấm, không thể khoá/mở khoá.");

            user.UpdateStatus(user.Status == EntityStatus.Paused ? EntityStatus.Active : EntityStatus.Paused);

            await _db.SaveChangesAsync(cancellationToken);

            return UserMapper.ToDto(user);
        }
    }
}
