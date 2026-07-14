using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Storage;

public sealed class AppPaths : IAppPaths
{
    public AppPaths(string? applicationDirectory = null, string? userDataDirectory = null)
    {
        ApplicationDirectory = Path.GetFullPath(applicationDirectory ?? AppContext.BaseDirectory);
        UserDataDirectory = Path.GetFullPath(userDataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TheGame"));
    }

    public string ApplicationDirectory { get; }

    public string UserDataDirectory { get; }

    public string ContentDatabasePath => Path.Combine(UserDataDirectory, "content.db");

    public string UserDatabasePath => Path.Combine(UserDataDirectory, "userdata.db");

    public string PlayersDirectory => Path.Combine(UserDataDirectory, "players");

    public string LegacyUsersDirectory => Path.Combine(ApplicationDirectory, "users");

    public string LegacyInventoryDirectory => Path.Combine(ApplicationDirectory, "inventory");

    public string LegacyEnemiesDirectory => Path.Combine(ApplicationDirectory, "enemies");
}
