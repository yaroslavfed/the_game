using Microsoft.EntityFrameworkCore;
using TheGame.Core.Battle;
using TheGame.Infrastructure.Persistence.Content;

namespace TheGame.Infrastructure.Battle;

public sealed class SqliteEnemyCatalog(IDbContextFactory<GameContentDbContext> contextFactory) : IEnemyCatalog
{
    public async Task<EnemyDefinition?> GetAsync(string rank, CancellationToken cancellationToken = default)
    {
        await using GameContentDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Enemies.AsNoTracking()
            .Where(enemy => enemy.Rank == rank && enemy.IsActive)
            .Select(enemy => new EnemyDefinition(
                enemy.Rank, enemy.Name, enemy.Health, enemy.Damage, enemy.Protection, enemy.Reward))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnemyDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using GameContentDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Enemies.AsNoTracking()
            .Where(enemy => enemy.IsActive)
            .OrderBy(enemy => enemy.SortOrder)
            .ThenBy(enemy => enemy.Rank)
            .Select(enemy => new EnemyDefinition(
                enemy.Rank, enemy.Name, enemy.Health, enemy.Damage, enemy.Protection, enemy.Reward))
            .ToArrayAsync(cancellationToken);
    }
}
