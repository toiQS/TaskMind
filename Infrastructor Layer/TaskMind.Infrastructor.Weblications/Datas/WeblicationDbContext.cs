using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Infrastructor.Weblications.Datas
{
    public class WeblicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentSessionProvider _sessionProvider;
        private readonly IPublisher _publisher;
        private readonly ILogger<WeblicationDbContext> _logger;

        public WeblicationDbContext(
            DbContextOptions<WeblicationDbContext> options,
            ICurrentSessionProvider sessionProvider,
            IPublisher publisher,
            ILogger<WeblicationDbContext> logger) : base(options)
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

        public DbSet<Chat> Chats { get; set; }

        public DbSet<JobApplication> JobApplications { get; set; }

        public DbSet<JobPosting> JobPostings { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<TestPaper> TestPapers { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<CompanySkillReflectionRequest> CompanySkillReflectionRequests { get; set; }
        public DbSet<SkillHistoryEntry> SkillHistoryEntries { get; set; }


        // ── OVERRIDE SAVECHANGESASYNC: XỬ LÝ TẬP TRUNG TẤT CẢ LOGIC ───────
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Guid? userId = _sessionProvider.GetUserId();
            List<AuditEntry> auditEntries = OnBeforeSaveChanges(userId);

            // 1) Lưu nghiệp vụ chính lần đầu
            int result = await base.SaveChangesAsync(cancellationToken);

            // 2) Lưu Audit Trail
            await OnAfterSaveChanges(auditEntries, cancellationToken);

            // 3) Publish domain event theo VÒNG LẶP: mỗi round có thể sinh thêm aggregate mới
            //    (ví dụ: CompanyVerifiedEventHandler tạo Notification -> Notification.Create lại
            //    raise NotificationCreatedEvent -> cần 1 round nữa để publish tiếp -> SendEmailEvent).
            while (true)
            {
                List<AggregateRoot> aggregates = ChangeTracker.Entries<AggregateRoot>()
                    .Where(e => e.Entity.DomainEvents.Any())
                    .Select(e => e.Entity)
                    .ToList();

                if (aggregates.Count == 0)
                    break;

                List<DomainEvent> domainEvents = aggregates.SelectMany(e => e.DomainEvents).ToList();

                foreach (AggregateRoot aggregate in aggregates)
                    aggregate.ClearDomainEvents();

                foreach (DomainEvent domainEvent in domainEvents)
                {
                    try
                    {
                        await _publisher.Publish(domainEvent, cancellationToken);
                        _logger.LogInformation("Successfully published domain event {EventType}", domainEvent.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish domain event {EventType}", domainEvent.GetType().Name);
                        foreach (AggregateRoot aggregate in aggregates)
                            aggregate.AddDomainEvent(domainEvent);
                        throw;
                    }
                }

                // Lưu các entity mới mà handler vừa Add (vd: Notification) TRƯỚC khi lặp lại vòng kế tiếp,
                // để domain event tiếp theo (NotificationCreatedEvent) được ghi nhận đúng.
                result += await base.SaveChangesAsync(cancellationToken);
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

}
