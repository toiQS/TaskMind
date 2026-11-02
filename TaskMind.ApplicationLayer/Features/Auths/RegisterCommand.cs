using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string CitizenId) : IRequest<ServiceResult<RegisteredAccountDto>>;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, ServiceResult<RegisteredAccountDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthenticationTokenService _tokenService;
    private readonly IEmailSender _emailSender;

    public RegisterHandler(
        IApplicationDbContext dbContext,
        IAuthenticationTokenService tokenService,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
    }

    public async Task<ServiceResult<RegisteredAccountDto>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Email?.Trim().ToLowerInvariant();
        var citizenId = command.CitizenId?.Trim();
        if (!AuthRules.IsValidEmail(email))
            return ServiceResult<RegisteredAccountDto>.Failure("Email không hợp lệ.");
        if (!AuthRules.IsValidPassword(command.Password))
            return ServiceResult<RegisteredAccountDto>.Failure("Mật khẩu phải có ít nhất 8 ký tự và tối đa 72 byte UTF-8.");
        if (citizenId is null || citizenId.Length != 12 || citizenId.Any(character => !char.IsDigit(character)))
            return ServiceResult<RegisteredAccountDto>.Failure("CitizenId phải gồm đúng 12 chữ số.");

        var emailExists = await _dbContext.Users
            .AnyAsync(user => user.Profile.Email.ToUpper() == email!.ToUpper(), cancellationToken);
        if (emailExists)
            return ServiceResult<RegisteredAccountDto>.Failure("Email đã được sử dụng.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, workFactor: 12);
        var createResult = User.CreateUser(citizenId, email!, passwordHash);
        if (!createResult.IsSuccess || createResult.Data is null)
            return ServiceResult<RegisteredAccountDto>.Failure(createResult.Message);

        var user = createResult.Data;
        var rawToken = _tokenService.CreateOpaqueToken();
        var tokenResult = user.Security.IssueEmailVerificationToken(
            _tokenService.HashToken(rawToken),
            DateTime.UtcNow.AddHours(24));
        if (!tokenResult.IsSuccess)
            return ServiceResult<RegisteredAccountDto>.Error("Không thể tạo mã xác minh email.");

        _dbContext.Users.Add(user);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Accounts_User_Email"
            })
        {
            return ServiceResult<RegisteredAccountDto>.Failure("Email đã được sử dụng.");
        }

        var sent = await _emailSender.SendAsync(
            email!,
            "Xác minh tài khoản TaskMind",
            $"<p>Mã xác minh của bạn: <strong>{System.Net.WebUtility.HtmlEncode(rawToken)}</strong></p><p>Mã có hiệu lực trong 24 giờ.</p>",
            cancellationToken);
        if (!sent)
            return ServiceResult<RegisteredAccountDto>.Error("Không gửi được email xác minh. Vui lòng yêu cầu gửi lại.");

        return ServiceResult<RegisteredAccountDto>.Success(new RegisteredAccountDto
        {
            AccountId = user.Id,
            Email = user.Profile.Email,
            IsVerified = user.IsVerified
        }, "Đăng ký thành công. Vui lòng kiểm tra email để xác minh tài khoản.");
    }
}
