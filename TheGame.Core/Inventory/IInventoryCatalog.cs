namespace TheGame.Core.Inventory;

public interface IInventoryCatalog
{
    Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
