using Microsoft.Extensions.DependencyInjection;

namespace TaskMind.Applications.Admins
{
    /// <summary>
    /// Đăng ký toàn bộ MediatR handlers của Application layer (Admin) — gọi từ Composition Root
    /// (WPF App.xaml.cs / Program.cs) cùng với IApplicationDbContext đã đăng ký ở Infrastructor layer:
    ///
    ///   services.AddApplicationAdmins();
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationAdmins(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            return services;
        }
    }
}
