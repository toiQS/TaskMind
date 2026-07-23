using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Commons.Cores
{
    public abstract class AuditableAggregateRoot : AggregateRoot, IAuditableEntity
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}
