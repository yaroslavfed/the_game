using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Players;
using TheGame.Infrastructure.Storage;
using Xunit;

namespace the_game.Tests.Storage;

public sealed class LegacyPlayerRepositoryTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        $"the-game-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task GetAsync_ReadsExistingEightLineProfile()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.LegacyUsersDirectory);
        await File.WriteAllLinesAsync(
            Path.Combine(paths.LegacyUsersDirectory, "7.txt"),
            ["Player", "12", "34", "500", "101", "101 102", "201", "201 202"],
            cancellationToken);
        var repository = new LegacyPlayerRepository(paths);

        PlayerProfile? profile = await repository.GetAsync("7", cancellationToken);

        Assert.NotNull(profile);
        Assert.Equal("Player", profile.Nickname);
        Assert.Equal(12, profile.Level);
        Assert.Equal(["101", "102"], profile.OwnedWeaponIds);
        Assert.Equal(["201", "202"], profile.OwnedArmorIds);
    }

    [Fact]
    public async Task SaveAsync_RoundTripsProfile()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        var repository = new LegacyPlayerRepository(paths);
        var expected = new PlayerProfile(
            "9", "Tester", 4, 25, 300, "101", ["101"], "201", ["201"]);

        await repository.SaveAsync(expected, cancellationToken);
        PlayerProfile? actual = await repository.GetAsync(expected.Id, cancellationToken);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Nickname, actual.Nickname);
        Assert.Equal(expected.Level, actual.Level);
        Assert.Equal(expected.Experience, actual.Experience);
        Assert.Equal(expected.Money, actual.Money);
        Assert.Equal(expected.EquippedWeaponId, actual.EquippedWeaponId);
        Assert.Equal(expected.OwnedWeaponIds, actual.OwnedWeaponIds);
        Assert.Equal(expected.EquippedArmorId, actual.EquippedArmorId);
        Assert.Equal(expected.OwnedArmorIds, actual.OwnedArmorIds);
    }

    [Fact]
    public async Task GetAsync_DoesNotOverwriteMalformedProfile()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        IAppPaths paths = CreatePaths();
        Directory.CreateDirectory(paths.LegacyUsersDirectory);
        string file = Path.Combine(paths.LegacyUsersDirectory, "3.txt");
        await File.WriteAllTextAsync(file, "broken", cancellationToken);
        var repository = new LegacyPlayerRepository(paths);

        await Assert.ThrowsAsync<DataFormatException>(() => repository.GetAsync("3", cancellationToken));

        Assert.Equal("broken", await File.ReadAllTextAsync(file, cancellationToken));
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
