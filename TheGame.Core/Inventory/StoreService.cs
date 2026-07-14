using TheGame.Core.Players;

namespace TheGame.Core.Inventory;

public sealed class StoreService(IPlayerRepository players, IInventoryCatalog catalog) : IStoreService
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<StoreResult> PurchaseAsync(string playerId, string itemId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            PlayerProfile? player = await players.GetAsync(playerId, cancellationToken);
            InventoryItem? item = await catalog.GetAsync(itemId, cancellationToken);
            if (player is null || item is null) return new(false, "Профиль или предмет не найден");
            IReadOnlyList<string> owned = item.Kind == InventoryItemKind.Weapon ? player.OwnedWeaponIds : player.OwnedArmorIds;
            if (owned.Contains(item.Id)) return new(false, "Предмет уже куплен");
            if (player.Money < item.Price) return new(false, "Недостаточно средств");

            PlayerProfile updated = item.Kind == InventoryItemKind.Weapon
                ? player with { Money = player.Money - item.Price, OwnedWeaponIds = [.. player.OwnedWeaponIds, item.Id] }
                : player with { Money = player.Money - item.Price, OwnedArmorIds = [.. player.OwnedArmorIds, item.Id] };
            await players.SaveAsync(updated, cancellationToken);
            return new(true, null);
        }
        finally { _gate.Release(); }
    }

    public async Task<StoreResult> EquipAsync(string playerId, string itemId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            PlayerProfile? player = await players.GetAsync(playerId, cancellationToken);
            InventoryItem? item = await catalog.GetAsync(itemId, cancellationToken);
            if (player is null || item is null) return new(false, "Профиль или предмет не найден");
            IReadOnlyList<string> owned = item.Kind == InventoryItemKind.Weapon ? player.OwnedWeaponIds : player.OwnedArmorIds;
            if (!owned.Contains(item.Id)) return new(false, "Сначала купите предмет");
            PlayerProfile updated = item.Kind == InventoryItemKind.Weapon
                ? player with { EquippedWeaponId = item.Id }
                : player with { EquippedArmorId = item.Id };
            await players.SaveAsync(updated, cancellationToken);
            return new(true, null);
        }
        finally { _gate.Release(); }
    }
}
