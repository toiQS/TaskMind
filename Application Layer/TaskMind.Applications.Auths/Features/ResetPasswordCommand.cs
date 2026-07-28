// ResetPasswordCommand.cs
using MediatR;
using TaskMind.Applications.Auths.Common;
using TaskMind.Applications.Auths.Interfaces;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Auths.Features
{
    /// <summary>Bước 3 (mục 4.1): đặt mật khẩu mới, xác thực + tiêu huỷ OTP lần cuối.</summary>
    public class ResetPasswordCommand : IRequest<Unit>
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IApplicationDbContext _db;
        private readonly IOtpService _otpService;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(IApplicationDbContext db, IOtpService otpService, IPasswordHasher passwordHasher)
        {
            _db = db;
            _otpService = otpService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                throw new InvalidOperationException("Mật khẩu phải có ít nhất 8 ký tự.");

            if (request.NewPassword != request.ConfirmPassword)
                throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

            var email = request.Email.Trim();

            if (!await _otpService.ValidateAndConsumeOtpAsync(email, request.OtpCode, cancellationToken))
                throw new InvalidOperationException("Mã xác thực không đúng hoặc đã hết hạn.");

            var account = await AccountLookup.FindByEmailAsync(_db, email, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            var newHash = _passwordHasher.Hash(request.NewPassword);
            var result = account.Security.UpdatePassword(newHash);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}