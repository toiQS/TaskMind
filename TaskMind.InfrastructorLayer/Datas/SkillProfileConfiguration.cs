using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class SkillProfileConfiguration : IEntityTypeConfiguration<SkillProfile>
{
    public void Configure(EntityTypeBuilder<SkillProfile> builder)
    {
        builder.HasIndex(profile => profile.UserId).IsUnique();
        builder.HasMany(profile => profile.Records)
            .WithOne()
            .HasForeignKey("SkillProfileId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.Records)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}