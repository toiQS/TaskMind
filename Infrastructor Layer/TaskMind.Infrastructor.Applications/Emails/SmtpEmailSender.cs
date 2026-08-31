// Infrastructor Layer/TaskMind.Infrastructor.Applications/Emails/SmtpEmailSender.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskMind.Applications.Commons;

namespace TaskMind.Infrastructor.Applications.Emails
{
    /// <summary>Cài đặt IEmailSender qua SMTP. Không throw ra ngoài để tránh chặn SaveChangesAsync/publish domain event (mục 6 - DDD note).</summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogWarning("Bỏ qua gửi email vì địa chỉ người nhận trống. Subject: {Subject}", subject);
                return;
            }

            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = _settings.EnableSsl
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Đã gửi email tới {To} - {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email tới {To} thất bại", to);
            }
        }
    }
}