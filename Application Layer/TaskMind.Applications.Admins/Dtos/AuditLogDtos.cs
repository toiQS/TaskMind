namespace TaskMind.Applications.Admins.Dtos
{
    public class AuditLogListItemDto
    {
        public Guid Id { get; set; }
        public Guid ActorAccountId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    /// <summary>Bộ lọc nhật ký kiểm toán (mục 4.21) — phục vụ truy vết tranh chấp/sự cố bảo mật.</summary>
    public class GetAuditLogsFilter
    {
        public Guid? ActorAccountId { get; set; }
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public DateTime? FromDateUtc { get; set; }
        public DateTime? ToDateUtc { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}