using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Commons.Cores
{
    public class AuditTrail
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string EntityName { get; set; } = null!;
        public string? PrimaryKey { get; set; }
        public TrailType TrailType { get; set; } = default!;
        public DateTimeOffset DateUtc { get; set; }

        public Dictionary<string, object?> OldValues { get; set; } = [];
        public Dictionary<string, object?> NewValues { get; set; } = [];
        public List<string> ChangedColumns { get; set; } = [];
    }
}
