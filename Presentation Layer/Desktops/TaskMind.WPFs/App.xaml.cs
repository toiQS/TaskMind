using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.IO;
using System.Windows;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Events;
using TaskMind.Infrastructor.Applications.Datas;

namespace TaskMind.WPFs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;
        public IServiceProvider ServiceProvider => host.Services;
        public IConfiguration Configuration { get; }

        public App()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            host = Host.CreateDefaultBuilder()
               .ConfigureServices((context, services) => ConfigureServices(services))
               .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("PostgreConnectString")
                ?? "Server=localhost:5432;Database=TaskMind;Username=postgres;Password=akai1234;";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<ICurrentSessionProvider, CurrentSessionProvider>(); // implement thật
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(CompanyVerifiedEvent).Assembly));

            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host) { await host.StopAsync(TimeSpan.FromSeconds(5)); }
            base.OnExit(e);
        }
    }
}   
