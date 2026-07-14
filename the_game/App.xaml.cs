using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Builder;
using System.Windows;
using the_game.Navigation;
using the_game.Lifecycle;
using the_game.ViewModels;
using the_game.Views;
using TheGame.Core.Storage;

namespace the_game;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(e.Args);
        builder.Services.AddTheGameDesktop();

        _host = builder.Build();

        RxAppBuilder.CreateReactiveUIBuilder()
            .WithExceptionHandler(_host.Services.GetRequiredService<IApplicationErrorService>())
            .WithCoreServices()
            .WithWpf()
            .BuildApp();

        await _host.StartAsync();
        await _host.Services.GetRequiredService<IContentDatabaseInitializer>().InitializeAsync();
        await _host.Services.GetRequiredService<IUserDatabaseInitializer>().InitializeAsync();

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
