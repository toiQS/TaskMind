// ConsoleEmailSender.cs
using TaskMind.Applications.Auths.Interfaces;

namespace TaskMind.Applications.Auths.Services
{
    /// <summary>Placeholder — chỉ ghi log, chưa gửi email thật. TODO: thay bằng SMTP/SendGrid... ở Infrastructure.</summary>
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"[OTP] Gửi tới {toEmail}: {otpCode}");
            return Task.CompletedTask;
        }
    }
}