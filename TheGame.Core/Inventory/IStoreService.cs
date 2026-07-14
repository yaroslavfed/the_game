namespace TheGame.Core.Inventory;

public sealed record StoreResult(bool IsSuccess, string? Error);

public interface IStoreService
{
    Task<StoreResult> PurchaseAsync(string playerId, string itemId, CancellationToken cancellationToken = default);
    Task<StoreResult> EquipAsync(string playerId, string itemId, CancellationToken cancellationToken = default);
}
