using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Battle;
using TheGame.Infrastructure.Inventory;
using TheGame.Infrastructure.Persistence.Content;
using Xunit;

namespace the_game.Tests.Storage;

public sealed class SqliteContentCatalogTests
{
    [Fact]
    public async Task Initializer_CreatesDatabaseConsumedBySqliteCatalogs()
    {
        using var paths = new TemporaryPaths();
        await File.WriteAllTextAsync(
            Path.Combine(paths.ApplicationDirectory, "content.seed.json"),
            """
            {
              "schemaVersion": 1,
              "contentVersion": "test",
              "items": [
                { "id": "101", "kind": "Weapon", "name": "Sword", "power": 15, "price": 200, "rarity": 2, "description": null }
              ],
              "enemies": [
                { "id": "raider", "rank": "2", "name": "Raider", "health": 125.5, "damage": 11, "protection": 3.5, "reward": 40 }
              ]
            }
            """,
            TestContext.Current.CancellationToken);

        await new ContentDatabaseInitializer(paths).InitializeAsync(TestContext.Current.CancellationToken);
        var factory = new ContentContextFactory(paths.ContentDatabasePath);
        var inventory = new SqliteInventoryCatalog(factory);
        var enemies = new SqliteEnemyCatalog(factory);

        Assert.Equal(
            new InventoryItem("101", InventoryItemKind.Weapon, 15, 200, 2),
            await inventory.GetAsync("101", TestContext.Current.CancellationToken));
        Assert.Equal("Raider", (await enemies.GetAsync("2", TestContext.Current.CancellationToken))?.Name);
    }

    [Fact]
    public async Task Initializer_AtomicallyUpdatesVersionAndKeepsRemovedItemAsTombstone()
    {
        using var paths = new TemporaryPaths();
        string seedPath = Path.Combine(paths.ApplicationDirectory, "content.seed.json");
        await File.WriteAllTextAsync(seedPath,
            Seed("1.0", """
            { "id": "101", "kind": "Weapon", "name": "Old sword", "power": 10, "price": 100, "rarity": 1, "description": null },
            { "id": "102", "kind": "Weapon", "name": "Current sword", "power": 20, "price": 200, "rarity": 2, "description": null }
            """), TestContext.Current.CancellationToken);
        var initializer = new ContentDatabaseInitializer(paths);
        await initializer.InitializeAsync(TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(seedPath,
            Seed("2.0", """
            { "id": "102", "kind": "Weapon", "name": "Updated sword", "power": 25, "price": 220, "rarity": 2, "description": null }
            """), TestContext.Current.CancellationToken);

        await initializer.InitializeAsync(TestContext.Current.CancellationToken);

        var catalog = new SqliteInventoryCatalog(new ContentContextFactory(paths.ContentDatabasePath));
        InventoryItem? removed = await catalog.GetAsync("101", TestContext.Current.CancellationToken);
        InventoryItem? updated = await catalog.GetAsync("102", TestContext.Current.CancellationToken);
        Assert.False(removed!.IsActive);
        Assert.Equal(25, updated!.Power);
        Assert.Equal(["102"], (await catalog.GetAllAsync(TestContext.Current.CancellationToken)).Select(item => item.Id));
    }

    private static string Seed(string version, string items) => $$"""
        {
          "schemaVersion": 1,
          "contentVersion": "{{version}}",
          "items": [{{items}}],
          "enemies": []
        }
        """;

    private sealed class ContentContextFactory(string databasePath) : IDbContextFactory<GameContentDbContext>
    {
        public GameContentDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<GameContentDbContext>()
                .UseSqlite($"Data Source={databasePath};Mode=ReadOnly")
                .Options;
            return new GameContentDbContext(options);
        }
    }

    private sealed class TemporaryPaths : IAppPaths, IDisposable
    {
        public TemporaryPaths()
        {
            ApplicationDirectory = Path.Combine(Path.GetTempPath(), $"the-game-content-{Guid.NewGuid():N}");
            Directory.CreateDirectory(ApplicationDirectory);
        }

        public string ApplicationDirectory { get; }
        public string UserDataDirectory => ApplicationDirectory;
        public string ContentDatabasePath => Path.Combine(ApplicationDirectory, "content.db");
        public string UserDatabasePath => Path.Combine(ApplicationDirectory, "userdata.db");
        public string PlayersDirectory => Path.Combine(ApplicationDirectory, "players");
        public string LegacyUsersDirectory => Path.Combine(ApplicationDirectory, "users");
        public string LegacyInventoryDirectory => Path.Combine(ApplicationDirectory, "inventory");
        public string LegacyEnemiesDirectory => Path.Combine(ApplicationDirectory, "enemies");

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(ApplicationDirectory, recursive: true);
        }
    }
}
