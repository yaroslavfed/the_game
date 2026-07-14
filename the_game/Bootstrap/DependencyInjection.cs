using Microsoft.Extensions.DependencyInjection;

namespace the_game;

public static class DependencyInjection
{
    public static IServiceCollection AddTheGameDesktop(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();

        return services;
    }
}
