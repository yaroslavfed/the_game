using System.Text;
using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Storage;

namespace TheGame.Infrastructure.Players;

public sealed class LegacyPlayerRepository(IAppPaths paths) : IPlayerRepository
{
    public async Task<PlayerProfile?> GetAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        string path = GetPath(playerId);
        if (!File.Exists(path))
        {
            return null;
        }

        string[] lines = await File.ReadAllLinesAsync(path, cancellationToken);
        LegacyTextParser.RequireLineCount(lines, 8, path);

        return new PlayerProfile(
            playerId,
            lines[0],
            LegacyTextParser.ParseInt(lines[1], "level", path),
            LegacyTextParser.ParseInt(lines[2], "experience", path),
            LegacyTextParser.ParseInt(lines[3], "money", path),
            lines[4],
            SplitIds(lines[5]),
            lines[6],
            SplitIds(lines[7]));
    }

    public async Task SaveAsync(
        PlayerProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        Directory.CreateDirectory(paths.LegacyUsersDirectory);
        string path = GetPath(profile.Id);
        string temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        string[] lines =
        [
            profile.Nickname,
            profile.Level.ToString(System.Globalization.CultureInfo.InvariantCulture),
            profile.Experience.ToString(System.Globalization.CultureInfo.InvariantCulture),
            profile.Money.ToString(System.Globalization.CultureInfo.InvariantCulture),
            profile.EquippedWeaponId,
            string.Join(' ', profile.OwnedWeaponIds),
            profile.EquippedArmorId,
            string.Join(' ', profile.OwnedArmorIds)
        ];

        try
        {
            await File.WriteAllLinesAsync(
                temporaryPath,
                lines,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private string GetPath(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId) ||
            playerId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            playerId is "." or "..")
        {
            throw new ArgumentException("Player ID is not a valid file identifier.", nameof(playerId));
        }

        return Path.Combine(paths.LegacyUsersDirectory, $"{playerId}.txt");
    }

    private static string[] SplitIds(string value) =>
        value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
