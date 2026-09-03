using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Infrastructor.Applications.Datas
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

        // ... (giữ nguyên toàn bộ DbSet<> như cũ) ...
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
        public DbSet<AuditLog> AuditLogs { get; }
        public DbSet<Review> Reviews { get; }
        public DbSet<Certificate> Certificates { get; }
        public DbSet<TestPaper> TestPapers { get; }
        public DbSet<Submission> Submissions { get; }
        public DbSet<CompanySkillReflectionRequest> CompanySkillReflectionRequests { get; set; }
        public DbSet<SkillHistoryEntry> SkillHistoryEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        // [CẬP NHẬT - fix] Toàn bộ quy trình (lưu nghiệp vụ + audit trail + N vòng publish event, mỗi
        // vòng có thể tạo thêm aggregate mới) giờ chạy trong MỘT transaction DB duy nhất. Trước đây
        // mỗi base.SaveChangesAsync() bên trong vòng lặp tự commit ngay lập tức — nếu publish domain
        // event ở vòng sau lỗi (ví dụ handler ném exception do bug/timeout), dữ liệu nghiệp vụ chính
        // và các Notification/AuditLog đã sinh ở các vòng trước đó vẫn nằm lại trong DB dù toàn bộ
        // thao tác coi như thất bại (throw ra ngoài) — không đảm bảo atomicity.
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await SaveChangesInternalAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<int> SaveChangesInternalAsync(CancellationToken cancellationToken)
        {
            Guid? userId = _sessionProvider.GetUserId();
            List<AuditEntry> auditEntries = OnBeforeSaveChanges(userId);

            // 1) Lưu nghiệp vụ chính lần đầu
            int result = await base.SaveChangesAsync(cancellationToken);

            // 2) Lưu Audit Trail
            await OnAfterSaveChanges(auditEntries, cancellationToken);

            // 3) Publish domain event theo VÒNG LẶP: mỗi round có thể sinh thêm aggregate mới.
            while (true)
            {
                List<AggregateRoot> aggregates = ChangeTracker.Entries<AggregateRoot>()
                    .Where(e => e.Entity.DomainEvents.Any())
                    .Select(e => e.Entity)
                    .ToList();

                if (aggregates.Count == 0)
                    break;

                // [MỚI - fix] Giữ nguyên map aggregate -> event của chính nó thay vì gộp phẳng rồi
                // sau đó (khi lỗi) gán NHẦM event vào MỌI aggregate không liên quan như code cũ.
                var eventsByAggregate = aggregates.ToDictionary(a => a, a => a.DomainEvents.ToList());

                foreach (AggregateRoot aggregate in aggregates)
                    aggregate.ClearDomainEvents();

                foreach (var (aggregate, events) in eventsByAggregate)
                {
                    foreach (DomainEvent domainEvent in events)
                    {
                        try
                        {
                            await _publisher.Publish(domainEvent, cancellationToken);
                            _logger.LogInformation("Successfully published domain event {EventType}", domainEvent.GetType().Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to publish domain event {EventType}", domainEvent.GetType().Name);
                            // Không cố "khôi phục" event vào aggregate nữa — transaction bên ngoài sẽ
                            // rollback toàn bộ, nên trạng thái in-memory không cần nhất quán tiếp.
                            throw;
                        }
                    }
                }

                // Lưu các entity mới mà handler vừa Add (vd: Notification) trước khi lặp vòng kế tiếp.
                result += await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        // Hàm chuẩn bị log Audit Trail trước khi lưu dữ liệu
        private List<AuditEntry> OnBeforeSaveChanges(Guid? userId)
        {
            ChangeTracker.DetectChanges();
            List<AuditEntry> auditEntries = [];

            // [MỚI - fix] Ngoài PasswordHash, loại thêm RefreshToken (bí mật phiên đăng nhập) và
            // CitizenId (số định danh công dân — dữ liệu cá nhân nhạy cảm, tài liệu mục 2.2 yêu cầu
            // mã hoá/giới hạn truy cập) khỏi audit trail — trước đây cả hai bị dump nguyên văn vào
            // cột jsonb của audit_trails.
            var sensitiveFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PasswordHash",
                "RefreshToken",
                "CitizenId"
            };

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

                    if (sensitiveFieldNames.Contains(prop.Metadata.Name))
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

            _ = await base.SaveChangesAsync(cancellationToken);
        }
    }
}