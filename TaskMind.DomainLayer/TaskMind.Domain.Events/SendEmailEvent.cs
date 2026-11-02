using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events
{
    public class SendEmailEvent : DomainEvent
    {
        public string To { get; }
        public string Subject { get; }
        public string Body { get; }
        public SendEmailEvent(string to, string subject, string body)
        {
            To = to;
            Subject = subject;
            Body = body;
        }
    }
}