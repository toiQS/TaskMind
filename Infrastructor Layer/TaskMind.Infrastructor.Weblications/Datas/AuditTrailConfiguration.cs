using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using TaskMind.Domain.Entities;

namespace TaskMind.Infrastructor.Weblications.Datas
{
    public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
    {
        public void Configure(EntityTypeBuilder<AuditTrail> builder)
        {
            _ = builder.ToTable("audit_trails");
            _ = builder.HasKey(e => e.Id);
            _ = builder.HasIndex(e => e.EntityName);

            _ = builder.Property(e => e.Id);
            _ = builder.Property(e => e.UserId);
            _ = builder.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            _ = builder.Property(e => e.PrimaryKey).HasMaxLength(100);
            _ = builder.Property(e => e.DateUtc).IsRequired();
            _ = builder.Property(e => e.TrailType).HasConversion<string>();

            _ = builder.Property(e => e.OldValues)
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                            v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions?)null)!
                        )
                        .HasColumnType("jsonb");

            _ = builder.Property(e => e.NewValues)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions?)null)!
                )
                .HasColumnType("jsonb");
            //builder.Property(e => e.ChangedColumns).HasColumnType("jsonb");
            _ = builder.Property(x => x.ChangedColumns)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!
                    )
                    .HasColumnType("jsonb");
        }
    }
}
