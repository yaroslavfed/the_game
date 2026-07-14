namespace TheGame.Core.Inventory;

public interface IInventoryCatalog
{
    Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default);
}
