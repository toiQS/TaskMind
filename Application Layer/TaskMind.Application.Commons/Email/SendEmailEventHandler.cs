// Application Layer/TaskMind.Application.Events/SendEmailEventHandler.cs
using MediatR;
using System.Net;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Commons.Email
{
    /// <summary>
    /// Thực thi việc gửi email qua IEmailSender, áp dụng biểu mẫu chung EmailTemplate cho mọi email
    /// hệ thống (mục 4.17). Đây là "cửa ra" duy nhất của kênh email — mọi context khác chỉ cần
    /// publish SendEmailEvent, không tự dựng SMTP/HTML riêng.
    /// </summary>
    public class SendEmailEventHandler : INotificationHandler<SendEmailEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendEmailEventHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Handle(SendEmailEvent notification, CancellationToken cancellationToken)
        {
            var bodyHtml = $"<p>{WebUtility.HtmlEncode(notification.Body).Replace("\n", "<br/>")}</p>";

            var html = EmailTemplate.Build(new EmailContent
            {
                Title = notification.Subject,
                BodyHtml = bodyHtml
            });

            await _emailSender.SendAsync(notification.To, notification.Subject, html, cancellationToken);
        }
    }
}