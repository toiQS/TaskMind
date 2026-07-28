// SendOtpCommand.cs
using MediatR;
using System.Text.RegularExpressions;
using TaskMind.Applications.Auths.Common;
using TaskMind.Applications.Auths.Interfaces;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Auths.Features
{
    /// <summary>Bước 1 (mục 4.1): gửi OTP về email. Dùng chung cho cả gửi lần đầu và "Gửi lại".</summary>
    public class SendOtpCommand : IRequest<Unit>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Unit>
    {
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private readonly IApplicationDbContext _db;
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;

        public SendOtpCommandHandler(IApplicationDbContext db, IOtpService otpService, IEmailSender emailSender)
        {
            _db = db;
            _otpService = otpService;
            _emailSender = emailSender;
        }

        public async Task<Unit> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex.IsMatch(request.Email))
                throw new InvalidOperationException("Email không hợp lệ.");

            var email = request.Email.Trim();

            // Không tiết lộ email có tồn tại hay không (chống dò quét tài khoản) — âm thầm bỏ qua nếu không tìm thấy.
            if (!await AccountLookup.ExistsByEmailAsync(_db, email, cancellationToken))
                return Unit.Value;

            var otp = await _otpService.GenerateOtpAsync(email, cancellationToken);
            await _emailSender.SendOtpEmailAsync(email, otp, cancellationToken);

            return Unit.Value;
        }
    }
}