using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.Domain.Commons.Events
{
    public interface IAuditableEntity
    {
        DateTimeOffset CreatedAtUtc { get; set; }
        DateTimeOffset? UpdatedAtUtc { get; set; }
        string CreatedBy { get; set; }
        string? UpdatedBy { get; set; }
    }
}
