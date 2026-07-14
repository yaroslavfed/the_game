using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using TheGame.Infrastructure.Persistence.Users;

namespace TheGame.Infrastructure.Players;

public sealed class SqlitePlayerRepository(IDbContextFactory<UserDataDbContext> contextFactory) : IPlayerRepository
{
    public async Task<PlayerProfile?> GetAsync(string playerId, CancellationToken cancellationToken = default)
    {
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        PlayerEntity? player = await context.Players.AsNoTracking()
            .Include(value => value.Items).Include(value => value.Loadout)
            .SingleOrDefaultAsync(value => value.Id == playerId, cancellationToken);
        return player is null ? null : Map(player);
    }

    public async Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        PlayerEntity player = await context.Players.Include(value => value.Items).Include(value => value.Loadout)
            .SingleOrDefaultAsync(value => value.Id == profile.Id, cancellationToken)
            ?? throw new InvalidOperationException("Player must be created together with an account.");

        player.Nickname = profile.Nickname;
        player.Level = profile.Level;
        player.Experience = profile.Experience;
        player.Money = profile.Money;
        context.Entry(player).Property(value => value.Revision).OriginalValue = profile.Revision;
        player.Revision = profile.Revision + 1;
        player.UpdatedAt = DateTimeOffset.UtcNow;
        SynchronizeItems(player, profile);
        player.Loadout ??= new PlayerLoadoutEntity { PlayerId = player.Id };
        player.Loadout.WeaponItemId = profile.EquippedWeaponId;
        player.Loadout.ArmorItemId = profile.EquippedArmorId;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new PlayerConcurrencyException(profile.Id, exception);
        }
    }

    internal static PlayerProfile Map(PlayerEntity player) => new(
        player.Id, player.Nickname, player.Level, player.Experience, player.Money,
        player.Loadout?.WeaponItemId ?? string.Empty,
        player.Items.Where(item => item.Kind == InventoryItemKind.Weapon).Select(item => item.ItemId).ToArray(),
        player.Loadout?.ArmorItemId ?? string.Empty,
        player.Items.Where(item => item.Kind == InventoryItemKind.Armor).Select(item => item.ItemId).ToArray(),
        player.Revision);

    private static void SynchronizeItems(PlayerEntity player, PlayerProfile profile)
    {
        var desired = profile.OwnedWeaponIds.Select(id => (id, InventoryItemKind.Weapon))
            .Concat(profile.OwnedArmorIds.Select(id => (id, InventoryItemKind.Armor))).ToDictionary(value => value.id);
        foreach (PlayerItemEntity removed in player.Items.Where(item => !desired.ContainsKey(item.ItemId)).ToArray())
            player.Items.Remove(removed);
        foreach ((string id, InventoryItemKind kind) in desired.Values)
            if (player.Items.All(item => item.ItemId != id))
                player.Items.Add(new PlayerItemEntity { PlayerId = player.Id, ItemId = id, Kind = kind, AcquiredAt = DateTimeOffset.UtcNow });
    }
}
