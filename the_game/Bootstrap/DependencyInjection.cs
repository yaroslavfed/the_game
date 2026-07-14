using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using TheGame.Infrastructure;
using the_game.Navigation;
using the_game.ViewModels;
using the_game.Views;
using the_game.Lifecycle;
using TheGame.Core.Players;
using the_game.Session;

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
        services.AddSingleton<IApplicationLifetime, WpfApplicationLifetime>();
        services.AddSingleton<UserSession>();
        services.AddSingleton<IUserSession>(provider => provider.GetRequiredService<UserSession>());

        services.AddTransient<SplashViewModel>();
        services.AddTransient<SplashView>();
        services.AddTransient<IViewFor<SplashViewModel>>(
            provider => provider.GetRequiredService<SplashView>());

        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<MainMenuView>();
        services.AddTransient<IViewFor<MainMenuViewModel>>(
            provider => provider.GetRequiredService<MainMenuView>());

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsView>();
        services.AddTransient<IViewFor<SettingsViewModel>>(
            provider => provider.GetRequiredService<SettingsView>());

        services.AddTransient<AboutViewModel>();
        services.AddTransient<AboutView>();
        services.AddTransient<IViewFor<AboutViewModel>>(
            provider => provider.GetRequiredService<AboutView>());

        services.AddTransient<AuthenticationViewModel>();
        services.AddTransient<AuthenticationView>();
        services.AddTransient<IViewFor<AuthenticationViewModel>>(
            provider => provider.GetRequiredService<AuthenticationView>());

        services.AddTransient<ProfileViewModel>();
        services.AddTransient<ProfileView>();
        services.AddTransient<IViewFor<ProfileViewModel>>(
            provider => provider.GetRequiredService<ProfileView>());

        services.AddTransient<StoreViewModel>();
        services.AddTransient<StoreView>();
        services.AddTransient<IViewFor<StoreViewModel>>(
            provider => provider.GetRequiredService<StoreView>());

        return services;
    }
}
