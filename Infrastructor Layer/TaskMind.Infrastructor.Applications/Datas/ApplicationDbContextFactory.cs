using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using TaskMind.Applications.Commons;

namespace TaskMind.Infrastructor.Applications.Datas
{
    // Infrastructor Layer/TaskMind.Infrastructor.Applications/Datas/ApplicationDbContextFactory.cs
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Server=localhost:5432;Database=TaskMind;Username=postgres;Password=akai123;");

            return new ApplicationDbContext(
                optionsBuilder.Options,
                new NullCurrentSessionProvider(),
                new NoopPublisher(),
                NullLogger<ApplicationDbContext>.Instance);
        }

        private class NullCurrentSessionProvider : ICurrentSessionProvider
        {
            public Guid? GetUserId() => null;
        }

        private class NoopPublisher : IPublisher
        {
            public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
                where TNotification : INotification => Task.CompletedTask;
        }
    }
}
