// Application Layer/TaskMind.Application.Commons/IEmailSender.cs
namespace TaskMind.ApplicationLayer.Commons
{
    /// <summary>Trừu tượng hoá kênh gửi email, để Application layer không phụ thuộc SMTP/3rd-party cụ thể.</summary>
    public interface IEmailSender
    {
        Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}