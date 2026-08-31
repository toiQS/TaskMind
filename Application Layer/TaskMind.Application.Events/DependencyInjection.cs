using Microsoft.Extensions.DependencyInjection;

namespace TaskMind.Applications.Events
{
    /// <summary>
    /// Đăng ký toàn bộ MediatR handlers của Application layer (Admin) — gọi từ Composition Root
    /// (WPF App.xaml.cs / Program.cs) cùng với IApplicationDbContext đã đăng ký ở Infrastructor layer:
    ///
    ///   services.AddApplicationAdmins();
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationEvents(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            return services;
        }
    }
}