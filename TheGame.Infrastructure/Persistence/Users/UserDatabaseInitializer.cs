using Microsoft.EntityFrameworkCore;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class UserDatabaseInitializer(
    IAppPaths paths,
    IDbContextFactory<UserDataDbContext> contextFactory,
    LegacyUserDataMigrator? migrator = null) : IUserDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(paths.UserDataDirectory);
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout = 5000;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE TABLE IF NOT EXISTS data_migrations (Id TEXT NOT NULL PRIMARY KEY, CompletedAt TEXT NOT NULL, Details TEXT NULL);",
            cancellationToken);
        if (migrator is not null) await migrator.MigrateAsync(cancellationToken);
    }
}
