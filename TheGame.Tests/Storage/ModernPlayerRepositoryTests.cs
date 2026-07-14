using System.Text.Json;
using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Players;
using TheGame.Infrastructure.Storage;
using Xunit;

namespace the_game.Tests.Storage;

public sealed class ModernPlayerRepositoryTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        $"the-game-modern-store-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAsync_WritesVersionedDocumentAndRoundTripsProfile()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        var repository = new JsonPlayerRepository(paths);
        PlayerProfile expected = CreateProfile(money: 250);

        await repository.SaveAsync(expected, token);
        PlayerProfile? actual = await repository.GetAsync(expected.Id, token);
        using JsonDocument json = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(paths.PlayersDirectory, "7.json"), token));

        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Money, actual.Money);
        Assert.Equal(expected.OwnedWeaponIds, actual.OwnedWeaponIds);
        Assert.Equal(expected.OwnedArmorIds, actual.OwnedArmorIds);
        Assert.Empty(Directory.EnumerateFiles(paths.PlayersDirectory, "*.tmp"));
    }

    [Fact]
    public async Task GetAsync_ImportsLegacyProfileWithoutChangingSource()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.LegacyUsersDirectory);
        string legacyPath = Path.Combine(paths.LegacyUsersDirectory, "7.txt");
        const string legacy = "Tester\n2\n10\n100\n101\n101\n201\n201\n";
        await File.WriteAllTextAsync(legacyPath, legacy, token);
        var repository = new MigratingPlayerRepository(
            new JsonPlayerRepository(paths),
            new LegacyPlayerRepository(paths));

        PlayerProfile? imported = await repository.GetAsync("7", token);
        await repository.SaveAsync(imported! with { Money = 500 }, token);

        Assert.Equal(100, imported!.Money);
        Assert.Equal(legacy, await File.ReadAllTextAsync(legacyPath, token));
        Assert.Equal(500, (await repository.GetAsync("7", token))!.Money);
        Assert.True(File.Exists(Path.Combine(paths.PlayersDirectory, "7.json")));
    }

    [Fact]
    public async Task GetAsync_RejectsUnknownSchemaVersion()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.PlayersDirectory);
        await File.WriteAllTextAsync(
            Path.Combine(paths.PlayersDirectory, "7.json"),
            """{"schemaVersion":99,"profile":null}""",
            token);

        await Assert.ThrowsAsync<DataFormatException>(
            () => new JsonPlayerRepository(paths).GetAsync("7", token));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    private IAppPaths CreatePaths() =>
        new AppPaths(_directory, Path.Combine(_directory, "data"));

    private static PlayerProfile CreateProfile(int money) =>
        new("7", "Tester", 2, 10, money, "101", ["101"], "201", ["201"]);
}
