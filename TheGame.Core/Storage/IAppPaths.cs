namespace TheGame.Core.Storage;

public interface IAppPaths
{
    string ApplicationDirectory { get; }

    string UserDataDirectory { get; }

    string PlayersDirectory { get; }

    string LegacyUsersDirectory { get; }

    string LegacyInventoryDirectory { get; }

    string LegacyEnemiesDirectory { get; }
}
