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

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private IAppPaths CreatePaths() => new AppPaths(_directory, Path.Combine(_directory, "data"));
}
