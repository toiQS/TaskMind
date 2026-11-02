using System.ComponentModel.DataAnnotations;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores
{
    public class EntityBase
    {
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; } = string.Empty;
        public DateTime CreateAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; private set; } = DateTime.UtcNow;
        public EntityStatus Status { get; private set; } = EntityStatus.Active;

        public void UpdateStatus(EntityStatus status)
        {
            Status = status;
            UpdateAt = DateTime.UtcNow;
        }

    }
}
