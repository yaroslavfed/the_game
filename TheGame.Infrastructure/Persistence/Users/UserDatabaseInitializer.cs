using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class UserDatabaseInitializer(
    IAppPaths paths,
    IDbContextFactory<UserDataDbContext> contextFactory,
    LegacyUserDataMigrator? migrator = null,
    ILogger<UserDatabaseInitializer>? logger = null) : IUserDatabaseInitializer
{
    private const string InitialMigration = "20260714105808_InitialUserSchema";
    private const string HighestWaveMigration = "20260714113950_AddPlayerHighestWave";
    private const string EfProductVersion = "10.0.9";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(paths.UserDataDirectory);
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        bool databaseExists = File.Exists(paths.UserDatabasePath);
        if (databaseExists)
            await VerifyIntegrityAsync(context, cancellationToken);

        string[] pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        if (databaseExists)
        {
            if (pendingMigrations.Length > 0)
            {
                string backupPath = BackupDatabase();
                logger?.LogInformation("Created user database backup at {BackupPath}", backupPath);
            }
        }

        await BaselineEnsureCreatedDatabaseAsync(context, cancellationToken);
        foreach (string migration in pendingMigrations)
            logger?.LogInformation("Applying user database migration {Migration}", migration);
        await context.Database.MigrateAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout = 5000;", cancellationToken);
        if (migrator is not null) await migrator.MigrateAsync(cancellationToken);
        await VerifyIntegrityAsync(context, cancellationToken);
    }

    private string BackupDatabase()
    {
        string backupDirectory = Path.Combine(paths.UserDataDirectory, "backups");
        Directory.CreateDirectory(backupDirectory);
        string backupPath = Path.Combine(
            backupDirectory,
            $"userdata-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.db");
        using var source = new SqliteConnection($"Data Source={paths.UserDatabasePath};Mode=ReadOnly;Pooling=False");
        using var destination = new SqliteConnection($"Data Source={backupPath};Mode=ReadWriteCreate;Pooling=False");
        source.Open();
        destination.Open();
        source.BackupDatabase(destination);
        return backupPath;
    }

    private static async Task VerifyIntegrityAsync(
        UserDataDbContext context,
        CancellationToken cancellationToken)
    {
        DbConnection connection = context.Database.GetDbConnection();
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = "PRAGMA quick_check;";
            string? result = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken));
            if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                throw new DataFormatException($"User database integrity check failed: {result ?? "unknown error"}.");
        }
        catch (SqliteException exception)
        {
            throw new DataFormatException("User database cannot be opened or is corrupted.", exception);
        }
        finally
        {
            if (connection.State != ConnectionState.Closed) await connection.CloseAsync();
        }
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
            if (await ColumnExistsAsync(connection, "players", "HighestWave", cancellationToken))
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({HighestWaveMigration}, {EfProductVersion});",
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

    private static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        string tableName,
        string columnName,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info($table) WHERE name = $column;";
        DbParameter tableParameter = command.CreateParameter();
        tableParameter.ParameterName = "$table";
        tableParameter.Value = tableName;
        command.Parameters.Add(tableParameter);
        DbParameter columnParameter = command.CreateParameter();
        columnParameter.ParameterName = "$column";
        columnParameter.Value = columnName;
        command.Parameters.Add(columnParameter);
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }
}
