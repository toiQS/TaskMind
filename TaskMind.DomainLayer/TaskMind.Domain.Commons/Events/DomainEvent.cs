using MediatR;

namespace TaskMind.DomainLayer.TaskMind.Domain.Commons.Events
{
    public class DomainEvent : INotification
    {
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    }
}
