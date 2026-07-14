using Microsoft.Extensions.DependencyInjection;
using TheGame.Core.Authentication;
using TheGame.Core.Battle;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Battle;
using TheGame.Infrastructure.Authentication;
using TheGame.Infrastructure.Inventory;
using TheGame.Infrastructure.Players;
using TheGame.Infrastructure.Storage;

namespace TheGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTheGameInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAppPaths, AppPaths>();
        services.AddSingleton<IPlayerRepository, LegacyPlayerRepository>();
        services.AddSingleton<IInventoryCatalog, LegacyInventoryCatalog>();
        services.AddSingleton<IEnemyCatalog, LegacyEnemyCatalog>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IStoreService, StoreService>();

        return services;
    }
}
