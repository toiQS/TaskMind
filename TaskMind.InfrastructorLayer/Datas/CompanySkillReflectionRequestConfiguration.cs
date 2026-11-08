using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class CompanySkillReflectionRequestConfiguration :
    IEntityTypeConfiguration<CompanySkillReflectionRequest>
{
    public void Configure(EntityTypeBuilder<CompanySkillReflectionRequest> builder)
    {
        builder.HasIndex(request => new
            {
                request.CompanyId,
                request.StaffAccountId,
                request.SkillId
            })
            .IsUnique()
            .HasFilter("\"RequestStatus\" IN (0, 1)");
    }
}
