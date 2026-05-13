using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Open_lab.Data;
using Open_lab.Services;
using Open_lab.ViewModels;
using Open_lab.Views;

namespace Open_lab
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private Window? _loginWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _serviceProvider = ConfigureServices();
            ShowLoginWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<OpenLabDbContext>(_ => 
            {
                var context = new OpenLabDbContextFactory().CreateDbContext(System.Array.Empty<string>());
                if (AppSession.UserId > 0)
                {
                    context.CurrentUserId = AppSession.UserId;
                }
                return context;
            });

            RegisterServicesByConvention(services);

            services.AddTransient<IViewModelFactory, ViewModelFactory>();
            services.AddTransient<INavigationService, NavigationService>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }

        private void ShowLoginWindow()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            var loginView = new LoginView();
            var loginWindow = new Window
            {
                Title = "تسجيل الدخول",
                Content = loginView,
                Width = 400,
                Height = 550,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                FlowDirection = FlowDirection.RightToLeft
            };

            loginView.DataContext = ActivatorUtilities.CreateInstance<LoginViewModel>(
                _serviceProvider,
                new System.Action(() => OpenMainWindowAfterLogin(loginWindow)));

            _loginWindow = loginWindow;
            MainWindow = loginWindow;
            loginWindow.Show();
        }

        private void OpenMainWindowAfterLogin(Window loginWindow)
        {
            if (_serviceProvider == null)
            {
                return;
            }

            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = mainWindow;
            mainViewModel.InitializeAfterLogin(
                AppSession.Username,
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                () => ReturnToLogin(mainWindow));

            mainWindow.Show();
            loginWindow.Close();
            _loginWindow = null;
        }

        private void ReturnToLogin(Window mainWindow)
        {
            ShowLoginWindow();
            mainWindow.Close();
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
