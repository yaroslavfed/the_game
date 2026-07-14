using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
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
using TheGame.Infrastructure.Persistence.Content;
using TheGame.Infrastructure.Persistence.Users;

namespace TheGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTheGameInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAppPaths, AppPaths>();
        services.AddPooledDbContextFactory<GameContentDbContext>((provider, options) =>
        {
            IAppPaths paths = provider.GetRequiredService<IAppPaths>();
            options.UseSqlite($"Data Source={paths.ContentDatabasePath};Mode=ReadOnly;Cache=Shared");
        });
        services.AddPooledDbContextFactory<UserDataDbContext>((provider, options) =>
        {
            IAppPaths paths = provider.GetRequiredService<IAppPaths>();
            options.UseSqlite($"Data Source={paths.UserDatabasePath};Foreign Keys=True;Default Timeout=5;Pooling=True");
        });
        services.AddSingleton<IContentDatabaseInitializer, ContentDatabaseInitializer>();
        services.AddSingleton<IUserDatabaseInitializer, UserDatabaseInitializer>();
        services.AddSingleton<LegacyPlayerRepository>();
        services.AddSingleton<JsonPlayerRepository>();
        services.AddSingleton<IPlayerRepository, SqlitePlayerRepository>();
        services.AddSingleton<IInventoryCatalog, SqliteInventoryCatalog>();
        services.AddSingleton<IEnemyCatalog, SqliteEnemyCatalog>();
        services.AddSingleton<IAuthenticationService, SqliteAuthenticationService>();
        services.AddSingleton<IStoreService, SqliteStoreService>();
        services.AddSingleton<IBattleEngine, BattleEngine>();

        return services;
    }
}
