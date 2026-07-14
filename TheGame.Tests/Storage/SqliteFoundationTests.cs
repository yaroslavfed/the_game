using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Infrastructure.Persistence.Content;
using TheGame.Infrastructure.Persistence.Users;
using Xunit;

namespace the_game.Tests.Storage;

public sealed class SqliteFoundationTests
{
    [Fact]
    public async Task ContentContext_CreatesCatalogSchema()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var options = new DbContextOptionsBuilder<GameContentDbContext>().UseSqlite(connection).Options;
        await using var context = new GameContentDbContext(options);

        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        context.Items.Add(new ContentItemEntity
        {
            Id = "weapon-1", Kind = InventoryItemKind.Weapon, Name = "Test sword", Power = 10, Price = 20
        });
        context.Enemies.Add(new EnemyEntity
        {
            Id = "enemy-1", Rank = "1", Name = "Test enemy", Health = 100, Damage = 10
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, await context.Items.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Enemies.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UserContext_CascadesPlayerAggregate()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var options = new DbContextOptionsBuilder<UserDataDbContext>().UseSqlite(connection).Options;
        await using var context = new UserDataDbContext(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var account = new AccountEntity
        {
            Id = "account-1", Login = "Player", NormalizedLogin = "PLAYER",
            PasswordHash = [1], PasswordSalt = [2], HashIterations = 1, CreatedAt = DateTimeOffset.UtcNow,
            Player = new PlayerEntity
            {
                Id = "player-1", Nickname = "Player", Level = 1, CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Items = [new PlayerItemEntity { ItemId = "weapon-1", AcquiredAt = DateTimeOffset.UtcNow }],
                Loadout = new PlayerLoadoutEntity { WeaponItemId = "weapon-1" }
            }
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await context.Players.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await context.PlayerItems.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await context.PlayerLoadouts.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
    }
}
