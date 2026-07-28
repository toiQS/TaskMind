using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class ToggleBanUserCommand : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
    }

    public class ToggleBanUserCommandHandler : IRequestHandler<ToggleBanUserCommand, UserDto>
    {
        private readonly IApplicationDbContext _db;

        public ToggleBanUserCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserDto> Handle(ToggleBanUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

            user.UpdateStatus(user.Status == EntityStatus.Blocked ? EntityStatus.Active : EntityStatus.Blocked);

            await _db.SaveChangesAsync(cancellationToken);

            return UserMapper.ToDto(user);
        }
    }
}
