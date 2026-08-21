using MediatR;
using TaskMind.Applications.Auths.Common;
using TaskMind.Applications.Auths.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Auths.Features
{
    public class LoginCommand : IRequest<ServiceResult<LoginResultDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginHandler : IRequestHandler<LoginCommand, ServiceResult<LoginResultDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public LoginHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult<LoginResultDto>.Failed("Vui lòng nhập đầy đủ email và mật khẩu.");

            var email = request.Email.Trim();

            var account = await AccountLookup.FindByEmailAsync(_db, email, cancellationToken);

            // Không phân biệt rõ "sai email" hay "sai mật khẩu" trong thông báo lỗi (chống dò quét tài khoản).
            if (account == null || !_passwordHasher.Verify(request.Password, account.Security.PasswordHash))
                return ServiceResult<LoginResultDto>.Unauthorized("Email hoặc mật khẩu không chính xác.");

            if (account.Status == EntityStatus.Blocked)
                return ServiceResult<LoginResultDto>.Forbidden("Tài khoản đã bị cấm.");

            if (account.Status == EntityStatus.Paused)
                return ServiceResult<LoginResultDto>.Forbidden("Tài khoản đang bị tạm khoá.");

            var accessToken = _tokenService.GenerateAccessToken(account.Id, account.Profile.Email, account.Role);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // TODO: RememberMe hiện chưa ảnh hưởng thời hạn token — Security.AccessRefreshToken() đang
            // fix cứng 2 giờ ở tầng Domain; cần bổ sung tham số thời hạn nếu muốn "Ghi nhớ đăng nhập" kéo dài hơn.
            var tokenResult = account.Security.AccessRefreshToken(refreshToken);
            if (!tokenResult.IsSuccess)
                return ServiceResult<LoginResultDto>.Error(tokenResult.Message);

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new LoginResultDto
            {
                AccountId = account.Id,
                Email = account.Profile.Email,
                FullName = $"{account.Profile.FirstName} {account.Profile.LastName}".Trim(),
                Role = account.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return ServiceResult<LoginResultDto>.Success(dto, "Đăng nhập thành công");
        }
    }
}