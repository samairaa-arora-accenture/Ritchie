using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Richie.Application.Authentication;
using Richie.Infrastructure;
using Richie.Infrastructure.Persistence;
using Richie.UI.Services;
using Richie.UI.ViewModels;
using Richie.UI.Views;
using Richie.UI.Views.Auth;
using Serilog;

namespace Richie.UI;

/// <summary>
/// Composition root and window lifecycle: splash → auth (signup/login) → main shell,
/// with logout returning to the auth window.
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        string logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Richie", "logs");
        Directory.CreateDirectory(logDirectory);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) => configuration
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.File(
                    Path.Combine(logDirectory, "richie-.log"),
                    rollingInterval: RollingInterval.Day))
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure();

                services.AddSingleton<AuthNavigationService>();
                services.AddSingleton<IAuthNavigation>(sp => sp.GetRequiredService<AuthNavigationService>());

                services.AddTransient<LoginViewModel>();
                services.AddTransient<SignupViewModel>();
                services.AddTransient<ForgotPasswordViewModel>();
                services.AddTransient<LoginPage>();
                services.AddTransient<SignupPage>();
                services.AddTransient<ForgotPasswordPage>();

                services.AddTransient<AuthWindow>();
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            Log.Fatal(args.Exception, "Unhandled UI exception");
            MessageBox.Show(args.Exception.ToString(), "Richie — startup error");
        };

        await _host.StartAsync();

        var splash = new SplashWindow();
        splash.Show();

        try
        {
            await Task.Run(() => _host.Services.GetRequiredService<IDatabaseInitializer>().Initialize());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization failed");
            MessageBox.Show(ex.ToString(), "Richie — database error");
            splash.Close();
            Shutdown();
            return;
        }

        ShowAuth();
        splash.Close();
    }

    private void ShowAuth()
    {
        var navigation = _host.Services.GetRequiredService<AuthNavigationService>();
        var window = _host.Services.GetRequiredService<AuthWindow>();

        EventHandler<AuthenticatedEventArgs>? onAuthenticated = null;
        onAuthenticated = (_, _) =>
        {
            navigation.Authenticated -= onAuthenticated;
            ShowMain();
            window.Close();
        };
        navigation.Authenticated += onAuthenticated;

        bool firstRun = !_host.Services.GetRequiredService<IAuthService>().AnyUserExists();
        window.Show();
        window.Start(firstRun);
    }

    private void ShowMain()
    {
        var main = _host.Services.GetRequiredService<MainWindow>();
        main.LogoutRequested += OnLogout;
        main.Show();
    }

    private void OnLogout(object? sender, EventArgs e)
    {
        var main = (MainWindow)sender!;
        main.LogoutRequested -= OnLogout;
        _host.Services.GetRequiredService<IUserSession>().SignOut();
        ShowAuth();
        main.Close();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
