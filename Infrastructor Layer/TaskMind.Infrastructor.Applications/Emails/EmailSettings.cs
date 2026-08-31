// Infrastructor Layer/TaskMind.Infrastructor.Applications/Emails/EmailSettings.cs
namespace TaskMind.Infrastructor.Applications.Emails
{
    /// <summary>Bind từ appsettings.json, section "Email".</summary>
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "TaskMind";
        public bool EnableSsl { get; set; } = true;
    }
}