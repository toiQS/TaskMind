using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class ProjectRepositoryConfiguration : IEntityTypeConfiguration<ProjectRepository>
{
    public void Configure(EntityTypeBuilder<ProjectRepository> builder)
    {
        builder.ToTable("ProjectRepositories");
        builder.Property(item => item.StorageRootRef).IsRequired();
        builder.HasOne<Project>().WithMany().HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.OwnsMany(item => item.Environments, environment =>
        {
            environment.ToTable("ProjectEnvironments");
            environment.WithOwner().HasForeignKey("RepositoryId");
            environment.HasKey(item => item.Id);
            environment.HasIndex("RepositoryId", nameof(ProjectEnvironment.Environment)).IsUnique();
            environment.Property(item => item.CurrentRevisionId).IsConcurrencyToken();
            environment.HasOne<ProjectRevision>().WithMany()
                .HasForeignKey(item => item.CurrentRevisionId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Navigation(item => item.Environments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ProjectRevisionConfiguration : IEntityTypeConfiguration<ProjectRevision>
{
    public void Configure(EntityTypeBuilder<ProjectRevision> builder)
    {
        builder.ToTable("ProjectRevisions");
        builder.Property(item => item.ManifestHash).IsRequired();
        builder.Property(item => item.Message).IsRequired();
        builder.HasOne<ProjectRepository>().WithMany().HasForeignKey(item => item.RepositoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Project>().WithMany().HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProjectRevision>().WithMany().HasForeignKey(item => item.ParentRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.OwnsMany(item => item.Files, file =>
        {
            file.ToTable("RevisionFileEntries");
            file.WithOwner().HasForeignKey("RevisionId");
            file.HasKey(item => item.Id);
            file.Property(item => item.Path).IsRequired();
            file.Property(item => item.StorageObjectRef).IsRequired();
            file.Property(item => item.ContentHash).IsRequired();
            file.HasIndex("RevisionId", nameof(RevisionFileEntry.Path)).IsUnique();
        });
        builder.OwnsMany(item => item.Reviews, review =>
        {
            review.ToTable("RevisionReviews");
            review.WithOwner().HasForeignKey(item => item.RevisionId);
            review.HasKey(item => item.Id);
            review.HasIndex(item => new { item.RevisionId, item.ReviewerAccountId }).IsUnique();
        });
        builder.Navigation(item => item.Files).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(item => item.Reviews).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class LocalProjectWorkspaceConfiguration : IEntityTypeConfiguration<LocalProjectWorkspace>
{
    public void Configure(EntityTypeBuilder<LocalProjectWorkspace> builder)
    {
        builder.ToTable("LocalProjectWorkspaces");
        builder.HasOne<ProjectRepository>().WithMany().HasForeignKey(item => item.RepositoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProjectRevision>().WithMany().HasForeignKey(item => item.BaseRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RevisionPromotionConfiguration : IEntityTypeConfiguration<RevisionPromotion>
{
    public void Configure(EntityTypeBuilder<RevisionPromotion> builder)
    {
        builder.ToTable("RevisionPromotions");
        builder.HasOne<ProjectRepository>().WithMany().HasForeignKey(item => item.RepositoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProjectRevision>().WithMany().HasForeignKey(item => item.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
