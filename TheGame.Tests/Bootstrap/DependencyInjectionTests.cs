using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using TheGame.Core.Battle;
using the_game.Lifecycle;
using the_game.Navigation;
using the_game.ViewModels;
using the_game.Views;
using Xunit;

namespace the_game.Tests.Bootstrap;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddTheGameDesktop_RegistersShellAndNavigation()
    {
        var services = new ServiceCollection();

        services.AddTheGameDesktop();

        ServiceDescriptor shellRegistration = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(ShellWindow));

        Assert.Equal(ServiceLifetime.Singleton, shellRegistration.Lifetime);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ShellViewModel));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(INavigationService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<SplashViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<MainMenuViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<SettingsViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<AboutViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<StoreViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<ModeSelectionViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IViewFor<BattleViewModel>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IBattleEngine));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAsyncDelay));
    }
}
