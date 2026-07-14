using TheGame.Core.Inventory;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Storage;

namespace TheGame.Infrastructure.Inventory;

public sealed class LegacyInventoryCatalog(IAppPaths paths) : IInventoryCatalog
{
    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(paths.LegacyInventoryDirectory)) return [];
        var items = new List<InventoryItem>();
        foreach (string file in Directory.EnumerateFiles(paths.LegacyInventoryDirectory, "*.txt").Order())
        {
            InventoryItem? item = await GetAsync(Path.GetFileNameWithoutExtension(file), cancellationToken);
            if (item is not null) items.Add(item);
        }
        return items;
    }
    public async Task<InventoryItem?> GetAsync(
        string itemId,
        CancellationToken cancellationToken = default)
    {
        string path = Path.Combine(paths.LegacyInventoryDirectory, $"{itemId}.txt");
        if (!File.Exists(path))
        {
            return null;
        }

        string[] lines = await File.ReadAllLinesAsync(path, cancellationToken);
        LegacyTextParser.RequireLineCount(lines, 3, path);

        InventoryItemKind kind = itemId.StartsWith('1')
            ? InventoryItemKind.Weapon
            : InventoryItemKind.Armor;

        return new InventoryItem(
            itemId,
            kind,
            LegacyTextParser.ParseInt(lines[0], "power", path),
            LegacyTextParser.ParseInt(lines[1], "price", path),
            LegacyTextParser.ParseInt(lines[2], "rarity", path));
    }
}
