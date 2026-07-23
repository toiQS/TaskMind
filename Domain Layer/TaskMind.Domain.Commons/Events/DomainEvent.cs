using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.Domain.Commons.Events
{
    public class DomainEvent : INotification
    {
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    }
}
