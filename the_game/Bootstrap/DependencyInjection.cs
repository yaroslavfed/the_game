using Microsoft.Extensions.DependencyInjection;
using TheGame.Infrastructure;

namespace the_game;

public static class DependencyInjection
{
    public static IServiceCollection AddTheGameDesktop(this IServiceCollection services)
    {
        services.AddTheGameInfrastructure();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
