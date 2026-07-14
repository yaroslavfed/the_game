using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class UserDatabaseInitializer(
    IAppPaths paths,
    IDbContextFactory<UserDataDbContext> contextFactory,
    LegacyUserDataMigrator? migrator = null) : IUserDatabaseInitializer
{
    private const string InitialMigration = "20260714105808_InitialUserSchema";
    private const string EfProductVersion = "10.0.9";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(paths.UserDataDirectory);
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await BaselineEnsureCreatedDatabaseAsync(context, cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout = 5000;", cancellationToken);
        if (migrator is not null) await migrator.MigrateAsync(cancellationToken);
    }

    private static async Task BaselineEnsureCreatedDatabaseAsync(
        UserDataDbContext context,
        CancellationToken cancellationToken)
    {
        DbConnection connection = context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        try
        {
            if (!await TableExistsAsync(connection, "accounts", cancellationToken) ||
                await TableExistsAsync(connection, "__EFMigrationsHistory", cancellationToken))
                return;

            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS data_migrations (Id TEXT NOT NULL PRIMARY KEY, CompletedAt TEXT NOT NULL, Details TEXT NULL);",
                cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (MigrationId TEXT NOT NULL PRIMARY KEY, ProductVersion TEXT NOT NULL);",
                cancellationToken);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({InitialMigration}, {EfProductVersion});",
                cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }
}
