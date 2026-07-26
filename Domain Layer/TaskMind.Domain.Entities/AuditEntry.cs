using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; set; }
        public Guid? UserId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? PrimaryKey { get; set; }
        public DateTimeOffset DateUtc { get; set; }
        public TrailType TrailType { get; set; } = TrailType.None;
        public Dictionary<string, object?> OldValues { get; set; } = [];
        public Dictionary<string, object?> NewValues { get; set; } = [];
        public List<string> ChangedColumns { get; set; } = [];

        public AuditTrail ToAuditTrail()
        {
            return new AuditTrail
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                EntityName = EntityName,
                PrimaryKey = PrimaryKey,
                DateUtc = DateUtc,
                TrailType = TrailType,
                OldValues = OldValues,
                NewValues = NewValues,
                ChangedColumns = ChangedColumns
            };
        }
    }
}
