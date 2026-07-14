using TheGame.Core.Authentication;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Authentication;
using TheGame.Infrastructure.Players;
using TheGame.Infrastructure.Storage;
using Xunit;

namespace the_game.Tests.Authentication;

public sealed class AuthenticationServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"the-game-auth-{Guid.NewGuid():N}");

    [Fact]
    public async Task RegisterAndSignIn_PersistsHashInsteadOfPassword()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        var service = new AuthenticationService(paths, new LegacyPlayerRepository(paths));

        AuthenticationResult registration = await service.RegisterAsync("player", "secret-123", token);
        AuthenticationResult login = await service.SignInAsync("player", "secret-123", token);
        string json = await File.ReadAllTextAsync(Path.Combine(paths.UserDataDirectory, "accounts.json"), token);

        Assert.True(registration.IsSuccess);
        Assert.True(login.IsSuccess);
        Assert.DoesNotContain("secret-123", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SignIn_MigratesLegacyAccountAfterSuccessfulPasswordCheck()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        Directory.CreateDirectory(paths.ApplicationDirectory);
        await File.WriteAllLinesAsync(Path.Combine(paths.ApplicationDirectory, "id.txt"),
            ["Login: legacy", "Password: old-password", "ID: 42"], token);
        var service = new AuthenticationService(paths, new LegacyPlayerRepository(paths));

        AuthenticationResult result = await service.SignInAsync("legacy", "old-password", token);

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.PlayerId);
        Assert.True(File.Exists(Path.Combine(paths.UserDataDirectory, "accounts.json")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }
}
