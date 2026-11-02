using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record LogoutCommand(Guid AccountId) : IRequest<ServiceResult>;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;

    public LogoutHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Id == command.AccountId,
            cancellationToken);
        if (account is null)
            return ServiceResult.NotFound("Không tìm thấy tài khoản.");

        account.Security.RevokeRefreshToken();
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Error("Phiên đăng nhập vừa được cập nhật. Vui lòng thử lại.");
        }

        return ServiceResult.Success("Đăng xuất thành công.");
    }
}
