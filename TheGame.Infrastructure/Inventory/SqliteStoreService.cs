using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Infrastructure.Persistence.Users;
using TheGame.Infrastructure.Persistence;

namespace TheGame.Infrastructure.Inventory;

public sealed class SqliteStoreService(
    IDbContextFactory<UserDataDbContext> contextFactory,
    IInventoryCatalog catalog) : IStoreService
{
    public async Task<StoreResult> PurchaseAsync(string playerId, string itemId, CancellationToken cancellationToken = default)
    {
        InventoryItem? item = await catalog.GetAsync(itemId, cancellationToken);
        if (item is null || !item.IsActive) return new(false, "Предмет недоступен");
        return await SqliteRetryPolicy.ExecuteAsync(
            token => PurchaseCoreAsync(playerId, item, token), cancellationToken);
    }

    private async Task<StoreResult> PurchaseCoreAsync(string playerId, InventoryItem item, CancellationToken cancellationToken)
    {
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        PlayerEntity? player = await context.Players.Include(value => value.Items)
            .SingleOrDefaultAsync(value => value.Id == playerId, cancellationToken);
        if (player is null) return new(false, "Профиль не найден");
        if (player.Items.Any(value => value.ItemId == item.Id)) return new(false, "Предмет уже куплен");
        if (player.Money < item.Price) return new(false, "Недостаточно средств");
        player.Money -= item.Price;
        player.Revision++;
        player.UpdatedAt = DateTimeOffset.UtcNow;
        player.Items.Add(new PlayerItemEntity
        {
            PlayerId = player.Id, ItemId = item.Id, Kind = item.Kind, AcquiredAt = DateTimeOffset.UtcNow, Source = "store"
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(true, null);
    }

    public async Task<StoreResult> EquipAsync(string playerId, string itemId, CancellationToken cancellationToken = default)
    {
        InventoryItem? item = await catalog.GetAsync(itemId, cancellationToken);
        if (item is null || !item.IsActive) return new(false, "Предмет недоступен");
        return await SqliteRetryPolicy.ExecuteAsync(
            token => EquipCoreAsync(playerId, item, token), cancellationToken);
    }

    private async Task<StoreResult> EquipCoreAsync(string playerId, InventoryItem item, CancellationToken cancellationToken)
    {
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        PlayerEntity? player = await context.Players.Include(value => value.Items).Include(value => value.Loadout)
            .SingleOrDefaultAsync(value => value.Id == playerId, cancellationToken);
        if (player is null) return new(false, "Профиль не найден");
        if (player.Items.All(value => value.ItemId != item.Id)) return new(false, "Сначала купите предмет");
        player.Loadout ??= new PlayerLoadoutEntity { PlayerId = player.Id };
        if (item.Kind == InventoryItemKind.Weapon) player.Loadout.WeaponItemId = item.Id;
        else player.Loadout.ArmorItemId = item.Id;
        player.Revision++;
        player.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return new(true, null);
    }
}
