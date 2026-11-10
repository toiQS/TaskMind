using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class TestPaperConfiguration : IEntityTypeConfiguration<TestPaper>
{
    public void Configure(EntityTypeBuilder<TestPaper> builder)
    {
        builder.OwnsMany(item => item.Questions, question =>
        {
            question.WithOwner().HasForeignKey("TestPaperId");
            question.HasKey(nameof(TestQuestion.Id));
            question.Ignore(item => item.Options);
            question.Property(item => item.Prompt).IsRequired();
            question.Property(item => item.Option0).IsRequired();
            question.Property(item => item.Option1).IsRequired();
            question.Property(item => item.Option2).IsRequired();
            question.Property(item => item.Option3).IsRequired();
        });
        builder.Navigation(item => item.Questions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
