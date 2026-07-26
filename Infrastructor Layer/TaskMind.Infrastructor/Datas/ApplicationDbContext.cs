using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using TaskMind.Application.Commons;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Infrastructor.Datas
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentSessionProvider _sessionProvider;
        private readonly IPublisher _publisher;
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentSessionProvider sessionProvider,
            IPublisher publisher,
            ILogger<ApplicationDbContext> logger) : base(options)
        {
            _sessionProvider = sessionProvider;
            _publisher = publisher;
            _logger = logger;
        }

        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<Admin> Admins { get; set; }

        public DbSet<AdminCompany> AdminCompanies { get; set; }

        public DbSet<AdminSchool> AdminSchools { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<ExchangeContract> ExchangeContracts { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectMember> ProjectMembers { get; set; }

        public DbSet<School> Schools { get; set; }

        public DbSet<Skill> Skills { get; set; }

        public DbSet<SkillLevelUpRequest> SkillLevelUpRequests { get; set; }

        public DbSet<SkillProfile> SkillProfiles { get; set; }

        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Student> Students { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<User> Users { get; set; }


        // ── OVERRIDE SAVECHANGESASYNC: XỬ LÝ TẬP TRUNG TẤT CẢ LOGIC ───────
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Quét tracker để tạo danh sách Audit Log (Trước khi lưu, trạng thái entity còn nguyên)
            Guid? userId = _sessionProvider.GetUserId();
            List<AuditEntry> auditEntries = OnBeforeSaveChanges(userId);

            // 2. Gom toàn bộ Domain Events từ các AggregateRoot trước khi lưu xuống DB
            List<AggregateRoot> aggregates = ChangeTracker.Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            List<DomainEvent> domainEvents = aggregates.SelectMany(e => e.DomainEvents).ToList();

            // Clear sạch event trong entity để tránh bị phát lặp lại ở lần Save sau
            foreach (AggregateRoot? aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            // 3. Thực hiện Lưu Dữ Liệu Nghiệp Vụ chính xuống database cứng lần 1
            int result = await base.SaveChangesAsync(cancellationToken);

            // 4. Cập nhật khóa ngoại của thực thể "Tạo mới" (được sinh tự động sau câu lệnh Save trên) và lưu Audit Trail
            await OnAfterSaveChanges(auditEntries, cancellationToken);

            // 5. Phát tán các Domain Events đi khắp hệ thống (Gửi thông báo, Realtime SignalR, Sync Elastic...)
            foreach (DomainEvent? domainEvent in domainEvents)
            {
                try
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                    _logger.LogInformation("Successfully published domain event {EventType}", domainEvent.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish domain event {EventType}", domainEvent.GetType().Name);

                    // Nếu gửi Event lỗi, nạp lại event vào entity để Handler lớp ngoài có thể bắt và xử lý retry
                    foreach (AggregateRoot? aggregateRoot in aggregates)
                    {
                        aggregateRoot.AddDomainEvent(domainEvent);
                    }
                    throw;
                }
            }

            return result;
        }

        // Hàm chuẩn bị log Audit Trail trước khi lưu dữ liệu
        private List<AuditEntry> OnBeforeSaveChanges(Guid? userId)
        {
            ChangeTracker.DetectChanges();
            List<AuditEntry> auditEntries = [];

            foreach (EntityEntry entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditTrail || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }

                AuditEntry auditEntry = new(entry)
                {
                    EntityName = entry.Entity.GetType().Name,
                    UserId = userId,
                    DateUtc = DateTimeOffset.UtcNow
                };

                auditEntries.Add(auditEntry);

                foreach (PropertyEntry prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.PrimaryKey = prop.CurrentValue?.ToString();
                        continue;
                    }

                    if (prop.Metadata.Name.Equals("PasswordHash"))
                    {
                        continue;
                    }

                    string name = prop.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.TrailType = TrailType.Create;
                            auditEntry.NewValues[name] = prop.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.TrailType = TrailType.Delete;
                            auditEntry.OldValues[name] = prop.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (!Equals(prop.OriginalValue, prop.CurrentValue))
                            {
                                auditEntry.TrailType = TrailType.Update;
                                auditEntry.ChangedColumns.Add(name);
                                auditEntry.OldValues[name] = prop.OriginalValue;
                                auditEntry.NewValues[name] = prop.CurrentValue;
                            }
                            break;
                    }
                }
            }

            return auditEntries.Where(x => x.TrailType != TrailType.None).ToList();
        }

        // Hàm xử lý điền ID cho các bản ghi vừa Insert mới và ghi đống log vào DB
        private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries == null || !auditEntries.Any())
            {
                return;
            }

            foreach (AuditEntry entry in auditEntries)
            {
                if (entry.TrailType == TrailType.Create)
                {
                    // Lấy lại Id thực tế mà database vừa sinh tự động ra để gắn vào log
                    foreach (PropertyEntry prop in entry.Entry.Properties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            entry.PrimaryKey = prop.CurrentValue?.ToString();
                            break;
                        }
                    }
                }

                _ = AuditTrails.Add(entry.ToAuditTrail());
            }

            // Lưu dữ liệu của bảng Audit Trails xuống DB
            _ = await base.SaveChangesAsync(cancellationToken);
        }
    }
    public class AuditEntry
    {
        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
        }

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; set; }
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
