using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class EntityRelationshipsConfiguration :
    IEntityTypeConfiguration<Company>,
    IEntityTypeConfiguration<School>,
    IEntityTypeConfiguration<AuditLog>,
    IEntityTypeConfiguration<Certificate>,
    IEntityTypeConfiguration<ExchangeContract>,
    IEntityTypeConfiguration<JobApplication>,
    IEntityTypeConfiguration<JobPosting>,
    IEntityTypeConfiguration<Message>,
    IEntityTypeConfiguration<Notification>,
    IEntityTypeConfiguration<OrganizationInvitation>,
    IEntityTypeConfiguration<Project>,
    IEntityTypeConfiguration<Review>,
    IEntityTypeConfiguration<SkillHistoryEntry>,
    IEntityTypeConfiguration<SkillLevelUpRequest>,
    IEntityTypeConfiguration<SkillProfile>,
    IEntityTypeConfiguration<Submission>,
    IEntityTypeConfiguration<TestPaper>,
    IEntityTypeConfiguration<CompanySkillReflectionRequest>,
    IEntityTypeConfiguration<Chat>,
    IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(item => item.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(item => item.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.ActorAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Submission>()
            .WithMany()
            .HasForeignKey(item => item.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<ExchangeContract> builder)
    {
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.PartyAAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.PartyBAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasOne<JobPosting>()
            .WithMany()
            .HasForeignKey(item => item.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(item => item.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasOne<Chat>()
            .WithMany(item => item.Messages)
            .HasForeignKey(item => item.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.SenderAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.RecipientAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.InvitedUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.InvitedByAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Navigation(item => item.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.ReviewerAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<SkillHistoryEntry> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(item => item.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.ResponsibleAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(item => item.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Submission>()
            .WithMany()
            .HasForeignKey(item => item.RelatedSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<SkillLevelUpRequest> builder)
    {
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(item => item.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.ApproverAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Submission>()
            .WithMany()
            .HasForeignKey(item => item.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<SkillProfile> builder)
    {
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<SkillProfile>(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasOne<TestPaper>()
            .WithMany()
            .HasForeignKey(item => item.TestPaperId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<TestPaper> builder)
    {
        // OwnerId is polymorphic (Company or School) and must remain an application-validated reference.
    }

    public void Configure(EntityTypeBuilder<CompanySkillReflectionRequest> builder)
    {
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(item => item.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.StaffAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(item => item.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.ResponsibleAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TestPaper>()
            .WithMany()
            .HasForeignKey(item => item.AssignedTestPaperId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Submission>()
            .WithMany()
            .HasForeignKey(item => item.VerificationSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        // MemberIds and primitive collection IDs are validated by the aggregate and are not foreign-key tables.
    }

    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasOne<School>()
            .WithMany()
            .HasForeignKey(item => item.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(item => item.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
