using Microsoft.Extensions.DependencyInjection;

namespace TaskMind.Applications.Auths
{
    /// <summary>
    /// Đăng ký MediatR handlers + services của module Auths — gọi từ Composition Root (WPF App.xaml.cs):
    ///   services.AddApplicationAuths();
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationAuths(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            

            return services;
        }
    }
}