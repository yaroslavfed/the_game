using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGame.Core.Inventory;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Persistence.Content;

public sealed class ContentDatabaseInitializer(
    IAppPaths paths,
    ILogger<ContentDatabaseInitializer>? logger = null) : IContentDatabaseInitializer
{
    private const int SupportedSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(paths.UserDataDirectory);
        ContentSeed seed = await LoadAndValidateSeedAsync(cancellationToken);
        IReadOnlyList<ContentItemEntity> tombstones = [];
        if (File.Exists(paths.ContentDatabasePath))
        {
            (ContentInfoEntity info, IReadOnlyList<ContentItemEntity> existingItems) =
                await ReadExistingDatabaseAsync(cancellationToken);
            if (info.SchemaVersion == seed.SchemaVersion && info.ContentVersion == seed.ContentVersion) return;
            var activeIds = seed.Items.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            tombstones = existingItems.Where(item => !activeIds.Contains(item.Id)).ToArray();
            logger?.LogInformation(
                "Updating content database from {OldVersion} to {NewVersion}", info.ContentVersion, seed.ContentVersion);
        }

        await BuildAndReplaceAsync(seed, tombstones, cancellationToken);
    }

    private async Task<ContentSeed> LoadAndValidateSeedAsync(CancellationToken cancellationToken)
    {
        string seedPath = Path.Combine(paths.ApplicationDirectory, "content.seed.json");
        if (!File.Exists(seedPath))
            throw new FileNotFoundException("Content seed was not deployed with the application.", seedPath);
        await using FileStream stream = File.OpenRead(seedPath);
        ContentSeed seed = await JsonSerializer.DeserializeAsync<ContentSeed>(stream, JsonOptions, cancellationToken)
            ?? throw new DataFormatException("Content seed is empty or invalid.");
        if (seed.SchemaVersion != SupportedSchemaVersion)
            throw new DataFormatException($"Unsupported content schema version {seed.SchemaVersion}.");
        if (string.IsNullOrWhiteSpace(seed.ContentVersion))
            throw new DataFormatException("Content version is required.");
        if (seed.Items is null || seed.Enemies is null)
            throw new DataFormatException("Content seed must contain item and enemy collections.");
        if (seed.Items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != seed.Items.Count)
            throw new DataFormatException("Content seed contains duplicate item IDs.");
        if (seed.Items.Any(item =>
                string.IsNullOrWhiteSpace(item.Id) || item.Id.Length > 64 ||
                string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 200 ||
                item.Description?.Length > 2000 || item.Power < 0 || item.Price < 0 || item.Rarity < 0 ||
                !Enum.IsDefined(item.Kind)))
            throw new DataFormatException("Content seed contains an invalid item.");
        if (seed.Enemies.Select(enemy => enemy.Id).Distinct(StringComparer.Ordinal).Count() != seed.Enemies.Count)
            throw new DataFormatException("Content seed contains duplicate enemy IDs.");
        if (seed.Enemies.Select(enemy => enemy.Rank).Distinct(StringComparer.Ordinal).Count() != seed.Enemies.Count)
            throw new DataFormatException("Content seed contains duplicate enemy ranks.");
        if (seed.Enemies.Any(enemy =>
                string.IsNullOrWhiteSpace(enemy.Id) || enemy.Id.Length > 64 ||
                string.IsNullOrWhiteSpace(enemy.Rank) || enemy.Rank.Length > 64 ||
                string.IsNullOrWhiteSpace(enemy.Name) || enemy.Name.Length > 200 ||
                !double.IsFinite(enemy.Health) || enemy.Health <= 0 ||
                !double.IsFinite(enemy.Damage) || enemy.Damage < 0 ||
                !double.IsFinite(enemy.Protection) || enemy.Protection < 0 || enemy.Reward < 0))
            throw new DataFormatException("Content seed contains an invalid enemy.");
        return seed;
    }

    private async Task<(ContentInfoEntity Info, IReadOnlyList<ContentItemEntity> Items)> ReadExistingDatabaseAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var options = new DbContextOptionsBuilder<GameContentDbContext>()
                .UseSqlite($"Data Source={paths.ContentDatabasePath};Mode=ReadOnly;Pooling=False")
                .Options;
            await using var context = new GameContentDbContext(options);
            await context.Database.OpenConnectionAsync(cancellationToken);
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PRAGMA quick_check;";
            string integrity = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken)) ?? "unknown error";
            if (!string.Equals(integrity, "ok", StringComparison.OrdinalIgnoreCase))
                throw new DataFormatException($"Content database integrity check failed: {integrity}.");
            ContentInfoEntity info = await context.ContentInfo.AsNoTracking().SingleAsync(cancellationToken);
            ContentItemEntity[] items = await context.Items.AsNoTracking().ToArrayAsync(cancellationToken);
            return (info, items);
        }
        catch (DataFormatException) { throw; }
        catch (Exception exception)
        {
            throw new DataFormatException("Content database cannot be read or is corrupted.", exception);
        }
    }

    private async Task BuildAndReplaceAsync(
        ContentSeed seed,
        IReadOnlyList<ContentItemEntity> tombstones,
        CancellationToken cancellationToken)
    {
        string temporaryPath = $"{paths.ContentDatabasePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var options = new DbContextOptionsBuilder<GameContentDbContext>()
                .UseSqlite($"Data Source={temporaryPath};Mode=ReadWriteCreate;Pooling=False").Options;
            await using (var context = new GameContentDbContext(options))
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
                context.ContentInfo.Add(new ContentInfoEntity
                {
                    SchemaVersion = seed.SchemaVersion, ContentVersion = seed.ContentVersion, CreatedAt = DateTimeOffset.UtcNow
                });
                context.Items.AddRange(seed.Items.Select((item, index) => Map(item, index)));
                context.Items.AddRange(tombstones.Select(item => new ContentItemEntity
                {
                    Id = item.Id, Kind = item.Kind, Name = item.Name, Power = item.Power, Price = item.Price,
                    Rarity = item.Rarity, Description = item.Description, IsActive = false, SortOrder = item.SortOrder
                }));
                context.Enemies.AddRange(seed.Enemies.Select((enemy, index) => new EnemyEntity
                {
                    Id = enemy.Id, Rank = enemy.Rank, Name = enemy.Name, Health = enemy.Health, Damage = enemy.Damage,
                    Protection = enemy.Protection, Reward = enemy.Reward, IsActive = true, SortOrder = index
                }));
                await context.SaveChangesAsync(cancellationToken);
            }
            if (File.Exists(paths.ContentDatabasePath)) File.Replace(temporaryPath, paths.ContentDatabasePath, null);
            else File.Move(temporaryPath, paths.ContentDatabasePath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private static ContentItemEntity Map(SeedItem item, int index) => new()
    {
        Id = item.Id, Kind = item.Kind, Name = item.Name, Power = item.Power, Price = item.Price,
        Rarity = item.Rarity, Description = item.Description, IsActive = true, SortOrder = index
    };

    private sealed record ContentSeed(
        int SchemaVersion,
        string ContentVersion,
        IReadOnlyList<SeedItem> Items,
        IReadOnlyList<SeedEnemy> Enemies);
    private sealed record SeedItem(string Id, InventoryItemKind Kind, string Name, int Power, int Price, int Rarity, string? Description);
    private sealed record SeedEnemy(string Id, string Rank, string Name, double Health, double Damage, double Protection, int Reward);
}
