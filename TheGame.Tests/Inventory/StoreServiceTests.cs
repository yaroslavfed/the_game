using TheGame.Core.Inventory;
using TheGame.Core.Players;
using Xunit;

namespace the_game.Tests.Inventory;

public sealed class StoreServiceTests
{
    private static readonly InventoryItem Weapon = new("102", InventoryItemKind.Weapon, 20, 300, 2);

    [Fact]
    public async Task PurchaseAsync_AddsItemAndDeductsPrice()
    {
        var players = new InMemoryPlayerRepository(CreatePlayer(money: 500));
        var service = new StoreService(players, new InMemoryCatalog(Weapon));

        StoreResult result = await service.PurchaseAsync("player", Weapon.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(200, players.Player.Money);
        Assert.Contains(Weapon.Id, players.Player.OwnedWeaponIds);
    }

    [Fact]
    public async Task PurchaseAsync_DoesNotChangeProfileWhenFundsAreInsufficient()
    {
        PlayerProfile original = CreatePlayer(money: 100);
        var players = new InMemoryPlayerRepository(original);
        var service = new StoreService(players, new InMemoryCatalog(Weapon));

        StoreResult result = await service.PurchaseAsync("player", Weapon.Id, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Same(original, players.Player);
        Assert.Equal(0, players.SaveCount);
    }

    [Fact]
    public async Task EquipAsync_EquipsOwnedItem()
    {
        var players = new InMemoryPlayerRepository(CreatePlayer(ownedWeapons: ["101", Weapon.Id]));
        var service = new StoreService(players, new InMemoryCatalog(Weapon));

        StoreResult result = await service.EquipAsync("player", Weapon.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(Weapon.Id, players.Player.EquippedWeaponId);
    }

    private static PlayerProfile CreatePlayer(int money = 500, IReadOnlyList<string>? ownedWeapons = null) =>
        new("player", "Tester", 1, 0, money, "101", ownedWeapons ?? ["101"], "201", ["201"]);

    private sealed class InMemoryPlayerRepository(PlayerProfile player) : IPlayerRepository
    {
        public PlayerProfile Player { get; private set; } = player;
        public int SaveCount { get; private set; }

        public Task<PlayerProfile?> GetAsync(string playerId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PlayerProfile?>(Player.Id == playerId ? Player : null);

        public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
        {
            Player = profile;
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryCatalog(params InventoryItem[] items) : IInventoryCatalog
    {
        public Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default) =>
            Task.FromResult(items.SingleOrDefault(item => item.Id == itemId));

        public Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InventoryItem>>(items);
    }
}
