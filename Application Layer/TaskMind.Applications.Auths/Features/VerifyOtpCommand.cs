// VerifyOtpCommand.cs
using MediatR;
using TaskMind.Applications.Auths.Interfaces;

namespace TaskMind.Applications.Auths.Features
{
    /// <summary>Bước 2 (mục 4.1): xác thực mã OTP 6 số. Không tiêu huỷ OTP ở bước này để bước 3 (đặt lại mật khẩu) còn xác thực lại lần cuối.</summary>
    public class VerifyOtpCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }

    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, bool>
    {
        private readonly IOtpService _otpService;

        public VerifyOtpCommandHandler(IOtpService otpService)
        {
            _otpService = otpService;
        }

        public async Task<bool> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.OtpCode) || request.OtpCode.Length != 6)
                throw new InvalidOperationException("Vui lòng nhập đủ 6 số mã xác thực.");

            var isValid = await _otpService.PeekOtpAsync(request.Email.Trim(), request.OtpCode, cancellationToken);
            if (!isValid)
                throw new InvalidOperationException("Mã xác thực không đúng hoặc đã hết hạn.");

            return true;
        }
    }
}