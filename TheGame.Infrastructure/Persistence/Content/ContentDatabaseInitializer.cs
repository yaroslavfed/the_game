using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Persistence.Content;

public sealed class ContentDatabaseInitializer(IAppPaths paths) : IContentDatabaseInitializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(paths.ContentDatabasePath)) return;

        string seedPath = Path.Combine(paths.ApplicationDirectory, "content.seed.json");
        if (!File.Exists(seedPath))
            throw new FileNotFoundException("Content seed was not deployed with the application.", seedPath);

        await using FileStream stream = File.OpenRead(seedPath);
        ContentSeed seed = await JsonSerializer.DeserializeAsync<ContentSeed>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("Content seed is empty or invalid.");

        string temporaryPath = $"{paths.ContentDatabasePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            {
                var options = new DbContextOptionsBuilder<GameContentDbContext>()
                    .UseSqlite($"Data Source={temporaryPath};Mode=ReadWriteCreate;Pooling=False")
                    .Options;
                await using var context = new GameContentDbContext(options);
                await context.Database.EnsureCreatedAsync(cancellationToken);
                context.ContentInfo.Add(new ContentInfoEntity
                {
                    SchemaVersion = seed.SchemaVersion,
                    ContentVersion = seed.ContentVersion,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                context.Items.AddRange(seed.Items.Select((item, index) => new ContentItemEntity
                {
                    Id = item.Id,
                    Kind = item.Kind,
                    Name = item.Name,
                    Power = item.Power,
                    Price = item.Price,
                    Rarity = item.Rarity,
                    Description = item.Description,
                    IsActive = true,
                    SortOrder = index
                }));
                context.Enemies.AddRange(seed.Enemies.Select((enemy, index) => new EnemyEntity
                {
                    Id = enemy.Id,
                    Rank = enemy.Rank,
                    Name = enemy.Name,
                    Health = enemy.Health,
                    Damage = enemy.Damage,
                    Protection = enemy.Protection,
                    Reward = enemy.Reward,
                    IsActive = true,
                    SortOrder = index
                }));
                await context.SaveChangesAsync(cancellationToken);
            }
            File.Move(temporaryPath, paths.ContentDatabasePath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private sealed record ContentSeed(
        int SchemaVersion,
        string ContentVersion,
        IReadOnlyList<SeedItem> Items,
        IReadOnlyList<SeedEnemy> Enemies);

    private sealed record SeedItem(
        string Id, InventoryItemKind Kind, string Name, int Power, int Price, int Rarity, string? Description);

    private sealed record SeedEnemy(
        string Id, string Rank, string Name, double Health, double Damage, double Protection, int Reward);
}
