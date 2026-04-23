using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Open_lab.Data;
using Open_lab.Services;
using Open_lab.ViewModels;
using Open_lab.Shell;

namespace Open_lab
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _serviceProvider = ConfigureServices();

            var mainWindow = new ShellWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<OpenLabDbContext>(_ => new OpenLabDbContextFactory().CreateDbContext(System.Array.Empty<string>()));

            RegisterServicesByConvention(services);

            services.AddTransient<IViewModelFactory, ViewModelFactory>();
            services.AddTransient<INavigationService, NavigationService>();
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }

        private static void RegisterServicesByConvention(IServiceCollection services)
        {
            var assembly = typeof(IAuthService).Assembly;
            var candidates = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "Open_lab.Services");

            foreach (var implementation in candidates)
            {
                var interfaceType = implementation.GetInterface($"I{implementation.Name}");
                if (interfaceType == null)
                {
                    continue;
                }

                services.AddTransient(interfaceType, implementation);
            }
        }
    }
}
