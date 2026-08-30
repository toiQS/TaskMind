using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root AuditLog [MỚI] — nhật ký thao tác nghiệp vụ quan trọng (mục 4.21).
    /// Khác với AuditTrail/AuditEntry (Commons.Cores) vốn là cơ chế kỹ thuật ghi vết thay đổi entity
    /// của EF Core interceptor (OldValues/NewValues/ChangedColumns theo cột dữ liệu); AuditLog ghi
    /// nhận ở mức nghiệp vụ: ai (ActorAccountId) đã làm gì (Action) trên đối tượng nào (EntityType/EntityId).
    /// </summary>
    [Index(nameof(EntityType), nameof(EntityId))]
    [Index(nameof(ActorAccountId), nameof(CreatedAtUtc))]
    public class AuditLog : AggregateRoot
    {
        public Guid ActorAccountId { get; private set; }

        /// <summary>Ví dụ: RoleChanged, Approved, PaymentIssued.</summary>
        public string Action { get; private set; } = string.Empty;
        public string EntityType { get; private set; } = string.Empty;
        public Guid EntityId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        private AuditLog() { }

        private AuditLog(Guid actorAccountId, string action, string entityType, Guid entityId)
        {
            ActorAccountId = actorAccountId;
            Action = action;
            EntityType = entityType;
            EntityId = entityId;
        }

        public static Result<AuditLog> Record(Guid actorAccountId, string action, string entityType, Guid entityId)
        {
            if (actorAccountId == Guid.Empty)
                return Result<AuditLog>.Failure("ActorAccountId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(action))
                return Result<AuditLog>.Failure("Action không được để trống.");
            if (string.IsNullOrWhiteSpace(entityType))
                return Result<AuditLog>.Failure("EntityType không được để trống.");

            return Result<AuditLog>.Success(new AuditLog(actorAccountId, action.Trim(), entityType.Trim(), entityId));
        }
    }
}
