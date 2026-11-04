using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.Property(skill => skill.SkillName).IsRequired();
        builder.Property(skill => skill.NormalizedName).IsRequired();
        builder.HasIndex(skill => skill.NormalizedName).IsUnique();
    }
}