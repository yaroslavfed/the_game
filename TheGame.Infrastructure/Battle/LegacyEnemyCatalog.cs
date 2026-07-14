using TheGame.Core.Battle;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Storage;

namespace TheGame.Infrastructure.Battle;

public sealed class LegacyEnemyCatalog(IAppPaths paths) : IEnemyCatalog
{
    public async Task<IReadOnlyList<EnemyDefinition>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(paths.LegacyEnemiesDirectory))
        {
            return [];
        }

        var enemies = new List<EnemyDefinition>();
        foreach (string file in Directory.EnumerateFiles(paths.LegacyEnemiesDirectory, "*.txt"))
        {
            EnemyDefinition? enemy = await GetAsync(Path.GetFileNameWithoutExtension(file), cancellationToken);
            if (enemy is not null)
            {
                enemies.Add(enemy);
            }
        }

        return enemies.OrderBy(enemy => enemy.Rank, StringComparer.Ordinal).ToArray();
    }

    public async Task<EnemyDefinition?> GetAsync(
        string rank,
        CancellationToken cancellationToken = default)
    {
        string path = Path.Combine(paths.LegacyEnemiesDirectory, $"{rank}.txt");
        if (!File.Exists(path))
        {
            return null;
        }

        string[] lines = await File.ReadAllLinesAsync(path, cancellationToken);
        LegacyTextParser.RequireLineCount(lines, 6, path);

        return new EnemyDefinition(
            lines[0],
            lines[1],
            LegacyTextParser.ParseDouble(lines[2], "health", path),
            LegacyTextParser.ParseDouble(lines[3], "damage", path),
            LegacyTextParser.ParseDouble(lines[4], "protection", path),
            LegacyTextParser.ParseInt(lines[5], "reward", path));
    }
}
