// Application Layer/TaskMind.Application.Events/SendEmailEventHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.ApplicationLayer.Commons.Emails
{
    /// <summary>
    /// Thực thi việc gửi email qua IEmailSender, áp dụng biểu mẫu chung EmailTemplate cho mọi email
    /// hệ thống (mục 4.17). Đây là "cửa ra" duy nhất của kênh email — mọi context khác chỉ cần
    /// publish SendEmailEvent, không tự dựng SMTP/HTML riêng.
    /// </summary>
    public class SendEmailEventHandler : INotificationHandler<SendEmailEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendEmailEventHandler> _logger;

        public SendEmailEventHandler(IEmailSender emailSender, ILogger<SendEmailEventHandler> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Handle(SendEmailEvent notification, CancellationToken cancellationToken)
        {
            var bodyHtml = $"<p>{WebUtility.HtmlEncode(notification.Body).Replace("\n", "<br/>")}</p>";

            var html = EmailTemplate.Build(new EmailContent
            {
                Title = notification.Subject,
                BodyHtml = bodyHtml
            });

            var sent = await _emailSender.SendAsync(notification.To, notification.Subject, html, cancellationToken);
            if (!sent)
                _logger.LogError(
                    "Không gửi được email domain event {EventType} tới {To}",
                    notification.GetType().Name,
                    notification.To);
        }
    }
}