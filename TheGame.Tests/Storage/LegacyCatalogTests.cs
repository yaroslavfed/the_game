using TheGame.Core.Battle;
using TheGame.Core.Inventory;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Battle;
using TheGame.Infrastructure.Inventory;
using TheGame.Infrastructure.Storage;
using Xunit;

namespace the_game.Tests.Storage;

public sealed class LegacyCatalogTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        $"the-game-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task InventoryCatalog_ReadsLegacyItem()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.LegacyInventoryDirectory);
        await File.WriteAllLinesAsync(
            Path.Combine(paths.LegacyInventoryDirectory, "101.txt"),
            ["15", "200", "2"],
            cancellationToken);

        InventoryItem? item = await new LegacyInventoryCatalog(paths).GetAsync("101", cancellationToken);

        Assert.Equal(new InventoryItem("101", InventoryItemKind.Weapon, 15, 200, 2), item);
    }

    [Fact]
    public async Task EnemyCatalog_ReadsLegacyEnemy()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.LegacyEnemiesDirectory);
        await File.WriteAllLinesAsync(
            Path.Combine(paths.LegacyEnemiesDirectory, "2.txt"),
            ["2", "Raider", "125.5", "11", "3.5", "40"],
            cancellationToken);

        EnemyDefinition? enemy = await new LegacyEnemyCatalog(paths).GetAsync("2", cancellationToken);

        Assert.Equal(new EnemyDefinition("2", "Raider", 125.5, 11, 3.5, 40), enemy);
    }

    [Fact]
    public async Task EnemyCatalog_GetAllAsync_ReadsEnemiesInRankOrder()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.LegacyEnemiesDirectory);
        await File.WriteAllLinesAsync(
            Path.Combine(paths.LegacyEnemiesDirectory, "2.txt"),
            ["2", "Raider", "120", "12", "3", "40"],
            cancellationToken);
        await File.WriteAllLinesAsync(
            Path.Combine(paths.LegacyEnemiesDirectory, "0.txt"),
            ["0", "Scout", "80", "8", "1", "15"],
            cancellationToken);

        IReadOnlyList<EnemyDefinition> enemies = await new LegacyEnemyCatalog(paths)
            .GetAllAsync(cancellationToken);

        Assert.Equal(["0", "2"], enemies.Select(enemy => enemy.Rank));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private IAppPaths CreatePaths() => new AppPaths(_directory, Path.Combine(_directory, "data"));
}
