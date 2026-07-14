using TheGame.Core.Inventory;

namespace TheGame.Infrastructure.Persistence.Content;

public sealed class ContentInfoEntity
{
    public int Id { get; set; } = 1;
    public int SchemaVersion { get; set; }
    public string ContentVersion { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ContentItemEntity
{
    public string Id { get; set; } = string.Empty;
    public InventoryItemKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Power { get; set; }
    public int Price { get; set; }
    public int Rarity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public sealed class EnemyEntity
{
    public string Id { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Health { get; set; }
    public double Damage { get; set; }
    public double Protection { get; set; }
    public int Reward { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
