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
