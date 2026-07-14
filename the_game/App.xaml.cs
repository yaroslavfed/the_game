using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using ReactiveUI.Builder;
using System.Windows;
using the_game.Navigation;
using the_game.ViewModels;
using the_game.Views;

namespace the_game;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(e.Args);
        builder.Services.AddTheGameDesktop();

        _host = builder.Build();
        await _host.StartAsync();

        MainWindow = _host.Services.GetRequiredService<ShellWindow>();
        INavigationService navigation = _host.Services.GetRequiredService<INavigationService>();
        await navigation.NavigateToAsync<SplashViewModel>();
        MainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
