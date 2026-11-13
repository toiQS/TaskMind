using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using TaskMind.ApplicationLayer.Commons;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TaskMind")
            ?? "Host=localhost;Database=TaskMind;Username=postgres";
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new NullCurrentSessionProvider(),
            new NoopPublisher(),
            new EphemeralDataProtectionProvider(),
            NullLogger<ApplicationDbContext>.Instance);
    }

    private sealed class NullCurrentSessionProvider : ICurrentSessionProvider
    {
        public Guid? GetUserId() => null;
    }

    private sealed class NoopPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification =>
            Task.CompletedTask;
    }
}
