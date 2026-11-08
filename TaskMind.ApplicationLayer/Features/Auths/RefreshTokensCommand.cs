using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record RefreshTokensCommand(string RefreshToken) : IRequest<ServiceResult<LoginResultDto>>;

public sealed class RefreshTokensHandler : IRequestHandler<RefreshTokensCommand, ServiceResult<LoginResultDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;

    public RefreshTokensHandler(IApplicationDbContext dbContext, IAuthenticationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<LoginResultDto>> Handle(
        RefreshTokensCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken) || command.RefreshToken.Length > 256)
            return ServiceResult<LoginResultDto>.Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");

        var tokenHash = _tokenService.HashToken(command.RefreshToken);
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Security.RefreshTokenHash == tokenHash,
            cancellationToken);
        if (account is null ||
            account.Security.IsRevoked ||
            account.Security.IsRefreshTokenExpired ||
            !account.Security.RefreshTokenExpiresAtUtc.HasValue)
            return ServiceResult<LoginResultDto>.Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
        if (!account.IsVerified || account.Status != EntityStatus.Active)
            return ServiceResult<LoginResultDto>.Forbidden("Tài khoản không đủ điều kiện để làm mới phiên đăng nhập.");
        if (!await AuthRules.IsOrganizationActive(_dbContext, account, cancellationToken))
            return ServiceResult<LoginResultDto>.Forbidden("Tổ chức liên kết với tài khoản không hoạt động.");

        var tokens = _tokenService.CreateTokenPair(account.Id, account.Profile.Email, account.Role);
        var rotateResult = account.Security.RotateRefreshToken(
            _tokenService.HashToken(tokens.RefreshToken),
            tokens.RefreshTokenExpiresAtUtc.UtcDateTime);
        if (!rotateResult.IsSuccess)
            return ServiceResult<LoginResultDto>.Error("Không thể làm mới phiên đăng nhập.");

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<LoginResultDto>.Unauthorized("Refresh token đã được sử dụng hoặc bị thu hồi.");
        }

        return ServiceResult<LoginResultDto>.Success(
            LoginHandler.CreateLoginResult(account, tokens),
            "Làm mới phiên đăng nhập thành công.");
    }
}
