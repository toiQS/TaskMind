// IEmailSender.cs
namespace TaskMind.Applications.Auths.Interfaces
{
    /// <summary>
    /// Gửi email cho các luồng xác thực (OTP quên mật khẩu...).
    /// TODO: implementation mặc định (ConsoleEmailSender) chỉ ghi log, cần thay bằng SMTP/SendGrid... thật ở Infrastructure.
    /// </summary>
    public interface IEmailSender
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default);
    }
}   