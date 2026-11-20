using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TaskMind.ApplicationLayer.Commons;

namespace TaskMind.InfrastructorLayer.Datas;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var searchDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        string? apiDirectory = null;
        while (searchDirectory != null)
        {
            var candidate = Path.Combine(searchDirectory.FullName, "TaskMind.APIs");
            if (Directory.Exists(candidate))
            {
                apiDirectory = candidate;
                break;
            }
            searchDirectory = searchDirectory.Parent;
        }
        apiDirectory ??= Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("PostgreConnectString");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:PostgreConnectString must be configured for EF Core design-time operations.");

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
