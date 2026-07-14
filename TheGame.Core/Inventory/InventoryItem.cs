namespace TheGame.Core.Inventory;

public enum InventoryItemKind
{
    Weapon,
    Armor
}

public sealed record InventoryItem(
    string Id,
    InventoryItemKind Kind,
    int Power,
    int Price,
    int Rarity);
