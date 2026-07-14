using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Authentication;
using TheGame.Infrastructure.Inventory;
using TheGame.Infrastructure.Persistence.Users;
using TheGame.Infrastructure.Players;
using TheGame.Infrastructure.Storage;
using Xunit;

namespace the_game.Tests.Storage;

public sealed class SqliteUserDatabaseTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"the-game-userdb-{Guid.NewGuid():N}");

    [Fact]
    public async Task Registration_PersistsAccountProfileInventoryAndHash()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        var factory = new UserContextFactory(paths.UserDatabasePath);
        await new UserDatabaseInitializer(paths, factory).InitializeAsync(token);
        var authentication = new SqliteAuthenticationService(factory);

        var registration = await authentication.RegisterAsync("Player", "secret-123", token);
        var signIn = await authentication.SignInAsync("player", "secret-123", token);
        var profile = await new SqlitePlayerRepository(factory).GetAsync(registration.PlayerId!, token);
        await using UserDataDbContext context = factory.CreateDbContext();
        AccountEntity account = await context.Accounts.AsNoTracking().SingleAsync(token);

        Assert.True(registration.IsSuccess);
        Assert.True(signIn.IsSuccess);
        Assert.Equal(["10"], profile!.OwnedWeaponIds);
        Assert.Equal(["20"], profile.OwnedArmorIds);
        Assert.NotEmpty(account.PasswordHash);
        Assert.NotEqual(System.Text.Encoding.UTF8.GetBytes("secret-123"), account.PasswordHash);
    }

    [Fact]
    public async Task Purchase_DeductsMoneyAndAddsItemInUserDatabase()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        var factory = new UserContextFactory(paths.UserDatabasePath);
        await new UserDatabaseInitializer(paths, factory).InitializeAsync(token);
        var authentication = new SqliteAuthenticationService(factory);
        string playerId = (await authentication.RegisterAsync("Player", "secret-123", token)).PlayerId!;
        await using (UserDataDbContext context = factory.CreateDbContext())
        {
            PlayerEntity player = await context.Players.SingleAsync(token);
            player.Money = 500;
            await context.SaveChangesAsync(token);
        }

        var service = new SqliteStoreService(factory, new Catalog(new InventoryItem("101", InventoryItemKind.Weapon, 20, 300, 2)));
        StoreResult result = await service.PurchaseAsync(playerId, "101", token);
        var profile = await new SqlitePlayerRepository(factory).GetAsync(playerId, token);

        Assert.True(result.IsSuccess);
        Assert.Equal(200, profile!.Money);
        Assert.Contains("101", profile.OwnedWeaponIds);
    }

    [Fact]
    public async Task Initializer_MigratesLegacyJsonOnlyOnce()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        var jsonPlayers = new JsonPlayerRepository(paths);
        var legacyPlayers = new LegacyPlayerRepository(paths);
        var oldAuthentication = new TheGame.Infrastructure.Authentication.AuthenticationService(paths, jsonPlayers);
        var oldRegistration = await oldAuthentication.RegisterAsync("LegacyPlayer", "secret-123", token);
        var factory = new UserContextFactory(paths.UserDatabasePath);
        var migrator = new LegacyUserDataMigrator(paths, factory, jsonPlayers, legacyPlayers);
        var initializer = new UserDatabaseInitializer(paths, factory, migrator);

        await initializer.InitializeAsync(token);
        await initializer.InitializeAsync(token);

        var login = await new SqliteAuthenticationService(factory).SignInAsync("legacyplayer", "secret-123", token);
        var profile = await new SqlitePlayerRepository(factory).GetAsync(oldRegistration.PlayerId!, token);
        await using UserDataDbContext context = factory.CreateDbContext();
        Assert.True(login.IsSuccess);
        Assert.NotNull(profile);
        Assert.Equal(1, await context.Accounts.CountAsync(token));
        Assert.Equal(1, await context.DataMigrations.CountAsync(token));
        Assert.True(File.Exists(Path.Combine(paths.UserDataDirectory, "accounts.json")));
    }

    [Fact]
    public async Task Initializer_BaselinesDatabasePreviouslyCreatedWithEnsureCreated()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        Directory.CreateDirectory(paths.UserDataDirectory);
        var factory = new UserContextFactory(paths.UserDatabasePath);
        await using (UserDataDbContext legacyContext = factory.CreateDbContext())
            await legacyContext.Database.EnsureCreatedAsync(token);

        await new UserDatabaseInitializer(paths, factory).InitializeAsync(token);

        await using UserDataDbContext context = factory.CreateDbContext();
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(token));
        Assert.Equal(1, await context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM __EFMigrationsHistory").SingleAsync(token));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    private sealed class UserContextFactory(string databasePath) : IDbContextFactory<UserDataDbContext>
    {
        public UserDataDbContext CreateDbContext() => new(
            new DbContextOptionsBuilder<UserDataDbContext>()
                .UseSqlite($"Data Source={databasePath};Foreign Keys=True;Pooling=False").Options);
    }

    private sealed class Catalog(InventoryItem item) : IInventoryCatalog
    {
        public Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<InventoryItem?>(item.Id == itemId ? item : null);
        public Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InventoryItem>>([item]);
    }
}
