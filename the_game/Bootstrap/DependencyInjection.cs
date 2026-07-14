using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using TheGame.Infrastructure;
using the_game.Navigation;
using the_game.ViewModels;
using the_game.Views;

namespace the_game;

public static class DependencyInjection
{
    public static IServiceCollection AddTheGameDesktop(this IServiceCollection services)
    {
        services.AddTheGameInfrastructure();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<ShellWindow>();
        services.AddSingleton<IViewLocator, DependencyInjectionViewLocator>();
        services.AddSingleton<INavigationService, ReactiveNavigationService>();
        services.AddSingleton<ILegacyNavigationBridge, LegacyNavigationBridge>();

        services.AddTransient<SplashViewModel>();
        services.AddTransient<SplashView>();
        services.AddTransient<IViewFor<SplashViewModel>>(
            provider => provider.GetRequiredService<SplashView>());

        // Transitional registration. Removed when the main menu becomes a routed view.
        services.AddTransient<start_page>();

        return services;
    }
}
