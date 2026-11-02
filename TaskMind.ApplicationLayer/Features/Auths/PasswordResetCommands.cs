using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record RequestPasswordResetCommand(
    string Email,
    AccountRole? Role = null,
    Guid? AccountId = null) : IRequest<ServiceResult>;

public sealed class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, ServiceResult>
{
    private const string GenericResponse =
        "Nếu tài khoản tồn tại, hướng dẫn đặt lại mật khẩu sẽ được gửi tới email đã đăng ký.";
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        IApplicationDbContext dbContext,
        IAuthenticationTokenService tokenService,
        IEmailSender emailSender,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ServiceResult> Handle(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        if (!AuthRules.IsValidEmail(command.Email) ||
            command.Role.HasValue && (!Enum.IsDefined(command.Role.Value) || command.Role == AccountRole.None) ||
            command.AccountId == Guid.Empty)
            return ServiceResult.Failure("Thông tin yêu cầu đặt lại mật khẩu không hợp lệ.");

        var email = command.Email.Trim().ToUpperInvariant();
        var accounts = await _dbContext.Accounts
            .Where(account =>
                account.Profile.Email.ToUpper() == email &&
                (!command.Role.HasValue || account.Role == command.Role.Value) &&
                (!command.AccountId.HasValue || account.Id == command.AccountId.Value) &&
                account.IsVerified)
            .Take(2)
            .ToListAsync(cancellationToken);
        if (accounts.Count != 1)
            return ServiceResult.Success(GenericResponse);

        var account = accounts[0];
        var rawToken = _tokenService.CreateOpaqueToken();
        var issueResult = account.Security.IssuePasswordResetToken(
            _tokenService.HashToken(rawToken),
            DateTime.UtcNow.AddMinutes(30));
        if (!issueResult.IsSuccess)
            return ServiceResult.Error("Không thể tạo mã đặt lại mật khẩu.");
        await _dbContext.SaveChangesAsync(cancellationToken);

        var sent = await _emailSender.SendAsync(
            account.Profile.Email,
            "Đặt lại mật khẩu TaskMind",
            $"<p>Mã đặt lại mật khẩu của bạn: <strong>{System.Net.WebUtility.HtmlEncode(rawToken)}</strong></p><p>Mã có hiệu lực trong 30 phút.</p>",
            cancellationToken);
        if (!sent)
            _logger.LogError("Không gửi được email đặt lại mật khẩu cho tài khoản {AccountId}", account.Id);

        return ServiceResult.Success(GenericResponse);
    }
}

public sealed record ConfirmPasswordResetCommand(
    string Email,
    string Token,
    string NewPassword) : IRequest<ServiceResult>;

public sealed class ConfirmPasswordResetHandler : IRequestHandler<ConfirmPasswordResetCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;

    public ConfirmPasswordResetHandler(
        IApplicationDbContext dbContext,
        IAuthenticationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult> Handle(
        ConfirmPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        if (!AuthRules.IsValidEmail(command.Email) ||
            string.IsNullOrWhiteSpace(command.Token) ||
            command.Token.Length > 256)
            return ServiceResult.Failure("Yêu cầu đặt lại mật khẩu không hợp lệ.");
        if (!AuthRules.IsValidPassword(command.NewPassword))
            return ServiceResult.Failure("Mật khẩu phải có ít nhất 8 ký tự và tối đa 72 byte UTF-8.");

        var email = command.Email.Trim().ToUpperInvariant();
        var tokenHash = _tokenService.HashToken(command.Token);
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Profile.Email.ToUpper() == email &&
                         candidate.Security.PasswordResetTokenHash == tokenHash,
            cancellationToken);
        if (account is null || !account.IsVerified)
            return ServiceResult.Unauthorized("Mã đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

        var consumeResult = account.Security.ConsumePasswordResetToken(tokenHash, DateTime.UtcNow);
        if (!consumeResult.IsSuccess)
            return ServiceResult.Unauthorized("Mã đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword, workFactor: 12);
        var updateResult = account.Security.UpdatePassword(passwordHash);
        if (!updateResult.IsSuccess)
            return ServiceResult.Failure(updateResult.Message);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Unauthorized("Mã đặt lại mật khẩu đã được sử dụng.");
        }

        return ServiceResult.Success("Đặt lại mật khẩu thành công. Hãy đăng nhập lại.");
    }
}
