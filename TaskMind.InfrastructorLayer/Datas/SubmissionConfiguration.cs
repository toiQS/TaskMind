using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.Property(submission => submission.Score).IsConcurrencyToken();
        builder.Property(submission => submission.SubmittedAtUtc).IsConcurrencyToken();
    }
}
