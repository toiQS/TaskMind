using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record LoginCommand(
    string Email,
    string Password,
    AccountRole? Role = null,
    Guid? AccountId = null) : IRequest<ServiceResult<LoginResultDto>>;

public sealed class LoginHandler : IRequestHandler<LoginCommand, ServiceResult<LoginResultDto>>
{
    private const string InvalidCredentialsMessage = "Email hoặc mật khẩu không chính xác.";
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;

    public LoginHandler(IApplicationDbContext dbContext, IAuthenticationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<LoginResultDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        if (!AuthRules.IsValidEmail(command.Email) || string.IsNullOrWhiteSpace(command.Password))
            return ServiceResult<LoginResultDto>.Unauthorized(InvalidCredentialsMessage);
        if (command.Role.HasValue && (!Enum.IsDefined(command.Role.Value) || command.Role == AccountRole.None))
            return ServiceResult<LoginResultDto>.Failure("Loại tài khoản không hợp lệ.");
        if (command.AccountId == Guid.Empty)
            return ServiceResult<LoginResultDto>.Failure("AccountId không hợp lệ.");

        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var candidates = await _dbContext.Accounts
            .Where(account =>
                account.Profile.Email.ToUpper() == normalizedEmail &&
                (!command.Role.HasValue || account.Role == command.Role.Value) &&
                (!command.AccountId.HasValue || account.Id == command.AccountId.Value))
            .ToListAsync(cancellationToken);

        var matchingAccounts = candidates
            .Where(account => AuthRules.PasswordMatches(command.Password, account.Security.PasswordHash))
            .Take(2)
            .ToList();
        if (matchingAccounts.Count == 0)
            return ServiceResult<LoginResultDto>.Unauthorized(InvalidCredentialsMessage);
        if (matchingAccounts.Count > 1)
            return ServiceResult<LoginResultDto>.Failure(
                "Email này liên kết với nhiều tài khoản. Hãy chọn Role hoặc AccountId để đăng nhập.");

        var account = matchingAccounts[0];
        if (!account.IsVerified)
            return ServiceResult<LoginResultDto>.Forbidden("Vui lòng xác minh email trước khi đăng nhập.");
        if (account.Status != EntityStatus.Active)
        {
            return ServiceResult<LoginResultDto>.Forbidden(account.Status switch
            {
                EntityStatus.Blocked => "Tài khoản đã bị cấm.",
                EntityStatus.Paused => "Tài khoản đang bị tạm khoá.",
                EntityStatus.Deleted => "Tài khoản đã bị xoá.",
                _ => "Tài khoản không ở trạng thái hoạt động."
            });
        }
        if (!await AuthRules.IsOrganizationActive(_dbContext, account, cancellationToken))
            return ServiceResult<LoginResultDto>.Forbidden("Tổ chức liên kết với tài khoản không hoạt động.");

        var tokens = _tokenService.CreateTokenPair(account.Id, account.Profile.Email, account.Role);
        var rotateResult = account.Security.RotateRefreshToken(
            _tokenService.HashToken(tokens.RefreshToken),
            tokens.RefreshTokenExpiresAtUtc.UtcDateTime);
        if (!rotateResult.IsSuccess)
            return ServiceResult<LoginResultDto>.Error("Không thể tạo phiên đăng nhập.");

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<LoginResultDto>.Error("Phiên đăng nhập vừa được cập nhật. Vui lòng thử lại.");
        }

        return ServiceResult<LoginResultDto>.Success(CreateLoginResult(account, tokens), "Đăng nhập thành công.");
    }

    internal static LoginResultDto CreateLoginResult(Account account, AuthenticationTokenPair tokens) => new()
    {
        AccountId = account.Id,
        Email = account.Profile.Email,
        FullName = AuthRules.FullName(account),
        Role = account.Role.ToString(),
        AccessToken = tokens.AccessToken,
        AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc,
        RefreshToken = tokens.RefreshToken,
        RefreshTokenExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
    };
}
