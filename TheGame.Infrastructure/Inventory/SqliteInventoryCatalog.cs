using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Infrastructure.Persistence.Content;

namespace TheGame.Infrastructure.Inventory;

public sealed class SqliteInventoryCatalog(IDbContextFactory<GameContentDbContext> contextFactory) : IInventoryCatalog
{
    public async Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default)
    {
        await using GameContentDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Items.AsNoTracking()
            .Where(item => item.Id == itemId && item.IsActive)
            .Select(item => new InventoryItem(item.Id, item.Kind, item.Power, item.Price, item.Rarity))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using GameContentDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Items.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => new InventoryItem(item.Id, item.Kind, item.Power, item.Price, item.Rarity))
            .ToArrayAsync(cancellationToken);
    }
}
