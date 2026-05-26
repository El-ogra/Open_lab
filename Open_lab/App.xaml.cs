using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Open_lab.Data;
using Open_lab.Services;
using Open_lab.ViewModels;
using Open_lab.Views;
using Open_lab.Views.Bootstrap;

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
            ShowStartupWindow();
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
                if (SessionContext.Current.UserId > 0)
                {
                    context.CurrentUserId = SessionContext.Current.UserId;
                }
                return context;
            });

            RegisterServicesByConvention(services);

            services.AddSingleton<ISessionContext>(SessionContext.Current);
            services.AddSingleton<IMutableSessionContext>(SessionContext.Current);
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<BootstrapViewModel>();

            return services.BuildServiceProvider();
        }

        private async void ShowStartupWindow()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            var adminSetupService = _serviceProvider.GetRequiredService<IAdminSetupService>();
            if (await adminSetupService.IsBootstrapRequiredAsync())
            {
                ShowBootstrapWindow();
                return;
            }

            ShowLoginWindow();
        }

        private void ShowBootstrapWindow()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            var bootstrapView = new BootstrapView();
            var bootstrapWindow = new Window
            {
                Title = "إعداد المشرف الأول",
                Content = bootstrapView,
                Width = 440,
                Height = 620,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                FlowDirection = FlowDirection.RightToLeft
            };

            bootstrapView.DataContext = ActivatorUtilities.CreateInstance<BootstrapViewModel>(
                _serviceProvider,
                new System.Action(() =>
                {
                    ShowLoginWindow();
                    bootstrapWindow.Close();
                }));

            MainWindow = bootstrapWindow;
            bootstrapWindow.Show();
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
                SessionContext.Current.Username,
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
