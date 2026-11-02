using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMind.ApplicationLayer.Commons;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record RequestEmailVerificationCommand(string Email) : IRequest<ServiceResult>;

public sealed class RequestEmailVerificationHandler : IRequestHandler<RequestEmailVerificationCommand, ServiceResult>
{
    private const string GenericResponse =
        "Nếu tài khoản cần xác minh tồn tại, email hướng dẫn sẽ được gửi tới địa chỉ đã đăng ký.";
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RequestEmailVerificationHandler> _logger;

    public RequestEmailVerificationHandler(
        IApplicationDbContext dbContext,
        IAuthenticationTokenService tokenService,
        IEmailSender emailSender,
        ILogger<RequestEmailVerificationHandler> logger)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ServiceResult> Handle(
        RequestEmailVerificationCommand command,
        CancellationToken cancellationToken)
    {
        if (!AuthRules.IsValidEmail(command.Email))
            return ServiceResult.Failure("Email không hợp lệ.");

        var email = command.Email.Trim().ToUpperInvariant();
        var account = await _dbContext.Users.SingleOrDefaultAsync(
            user => user.Profile.Email.ToUpper() == email,
            cancellationToken);
        if (account is null || account.IsVerified)
            return ServiceResult.Success(GenericResponse);

        var rawToken = _tokenService.CreateOpaqueToken();
        var issueResult = account.Security.IssueEmailVerificationToken(
            _tokenService.HashToken(rawToken),
            DateTime.UtcNow.AddHours(24));
        if (!issueResult.IsSuccess)
            return ServiceResult.Error("Không thể tạo mã xác minh email.");
        await _dbContext.SaveChangesAsync(cancellationToken);

        var sent = await _emailSender.SendAsync(
            account.Profile.Email,
            "Xác minh tài khoản TaskMind",
            $"<p>Mã xác minh của bạn: <strong>{System.Net.WebUtility.HtmlEncode(rawToken)}</strong></p><p>Mã có hiệu lực trong 24 giờ.</p>",
            cancellationToken);
        if (!sent)
            _logger.LogError("Không gửi được email xác minh cho tài khoản {AccountId}", account.Id);

        return ServiceResult.Success(GenericResponse);
    }
}

public sealed record ConfirmEmailVerificationCommand(string Email, string Token) : IRequest<ServiceResult>;

public sealed class ConfirmEmailVerificationHandler :
    IRequestHandler<ConfirmEmailVerificationCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;

    public ConfirmEmailVerificationHandler(
        IApplicationDbContext dbContext,
        IAuthenticationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult> Handle(
        ConfirmEmailVerificationCommand command,
        CancellationToken cancellationToken)
    {
        if (!AuthRules.IsValidEmail(command.Email) ||
            string.IsNullOrWhiteSpace(command.Token) ||
            command.Token.Length > 256)
            return ServiceResult.Failure("Mã xác minh không hợp lệ hoặc đã hết hạn.");

        var email = command.Email.Trim().ToUpperInvariant();
        var tokenHash = _tokenService.HashToken(command.Token);
        var account = await _dbContext.Users.SingleOrDefaultAsync(
            user => user.Profile.Email.ToUpper() == email &&
                    user.Security.EmailVerificationTokenHash == tokenHash,
            cancellationToken);
        if (account is null)
            return ServiceResult.Unauthorized("Mã xác minh không hợp lệ hoặc đã hết hạn.");

        var consumeResult = account.Security.ConsumeEmailVerificationToken(tokenHash, DateTime.UtcNow);
        if (!consumeResult.IsSuccess)
            return ServiceResult.Unauthorized("Mã xác minh không hợp lệ hoặc đã hết hạn.");
        var verifyResult = account.Verify();
        if (!verifyResult.IsSuccess)
            return ServiceResult.Failure(verifyResult.Message);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Unauthorized("Mã xác minh đã được sử dụng.");
        }

        return ServiceResult.Success("Xác minh email thành công.");
    }
}
