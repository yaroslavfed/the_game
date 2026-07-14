using System.Text.Json;
using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Storage;

namespace TheGame.Infrastructure.Players;

public sealed class JsonPlayerRepository(IAppPaths paths) : IPlayerRepository
{
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<PlayerProfile?> GetAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        string path = GetPath(playerId);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            await using FileStream stream = File.OpenRead(path);
            PlayerDocument? document = await JsonSerializer.DeserializeAsync<PlayerDocument>(
                stream,
                Options,
                cancellationToken);
            if (document is null)
            {
                throw new DataFormatException($"Player store '{path}' is empty or incomplete.");
            }
            if (document.SchemaVersion != CurrentSchemaVersion)
            {
                throw new DataFormatException(
                    $"Player store '{path}' uses unsupported schema version {document.SchemaVersion}.");
            }
            if (document.Profile is null)
            {
                throw new DataFormatException($"Player store '{path}' is empty or incomplete.");
            }
            if (!string.Equals(document.Profile.Id, playerId, StringComparison.Ordinal))
            {
                throw new DataFormatException($"Player ID in '{path}' does not match its file name.");
            }
            return document.Profile;
        }
        catch (JsonException exception)
        {
            throw new DataFormatException($"Player store '{path}' contains invalid JSON.", exception);
        }
    }

    public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return AtomicJsonFile.WriteAsync(
            GetPath(profile.Id),
            new PlayerDocument(CurrentSchemaVersion, profile),
            Options,
            cancellationToken);
    }

    private string GetPath(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId) ||
            playerId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            playerId is "." or "..")
        {
            throw new ArgumentException("Player ID is not a valid file identifier.", nameof(playerId));
        }
        return Path.Combine(paths.PlayersDirectory, $"{playerId}.json");
    }

    private sealed record PlayerDocument(int SchemaVersion, PlayerProfile Profile);
}
