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
    public async Task PlayerRepository_RejectsStaleProfileSave()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        var factory = new UserContextFactory(paths.UserDatabasePath);
        await new UserDatabaseInitializer(paths, factory).InitializeAsync(token);
        string playerId = (await new SqliteAuthenticationService(factory)
            .RegisterAsync("Player", "secret-123", token)).PlayerId!;
        var repository = new SqlitePlayerRepository(factory);
        var first = await repository.GetAsync(playerId, token);
        var stale = await repository.GetAsync(playerId, token);

        await repository.SaveAsync(first! with { Money = 100 }, token);

        await Assert.ThrowsAsync<TheGame.Core.Players.PlayerConcurrencyException>(
            () => repository.SaveAsync(stale! with { Money = 200 }, token));
        Assert.Equal(100, (await repository.GetAsync(playerId, token))!.Money);
    }

    [Fact]
    public async Task ParallelPurchase_ChargesAndGrantsItemOnlyOnce()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        var factory = new UserContextFactory(paths.UserDatabasePath);
        await new UserDatabaseInitializer(paths, factory).InitializeAsync(token);
        string playerId = (await new SqliteAuthenticationService(factory)
            .RegisterAsync("Player", "secret-123", token)).PlayerId!;
        await using (UserDataDbContext context = factory.CreateDbContext())
        {
            PlayerEntity player = await context.Players.SingleAsync(token);
            player.Money = 500;
            await context.SaveChangesAsync(token);
        }
        var service = new SqliteStoreService(
            factory, new Catalog(new InventoryItem("101", InventoryItemKind.Weapon, 20, 300, 2)));

        StoreResult[] results = await Task.WhenAll(
            service.PurchaseAsync(playerId, "101", token),
            service.PurchaseAsync(playerId, "101", token));

        var profile = await new SqlitePlayerRepository(factory).GetAsync(playerId, token);
        Assert.Single(results, result => result.IsSuccess);
        Assert.Equal(200, profile!.Money);
        Assert.Equal(1, profile.OwnedWeaponIds.Count(id => id == "101"));
    }

    [Fact]
    public async Task Initializer_MigratesLegacyTextOnlyOnce()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        Directory.CreateDirectory(paths.ApplicationDirectory);
        Directory.CreateDirectory(paths.LegacyUsersDirectory);
        await File.WriteAllLinesAsync(Path.Combine(paths.ApplicationDirectory, "id.txt"),
            ["Login: LegacyPlayer", "Password: secret-123", "ID: 42"], token);
        await File.WriteAllLinesAsync(Path.Combine(paths.LegacyUsersDirectory, "42.txt"),
            ["Legacy Hero", "3", "25", "500", "101", "101 102", "201", "201"], token);
        var factory = new UserContextFactory(paths.UserDatabasePath);
        var migrator = new LegacyUserDataMigrator(paths, factory);
        var initializer = new UserDatabaseInitializer(paths, factory, migrator);

        await initializer.InitializeAsync(token);
        await initializer.InitializeAsync(token);

        var login = await new SqliteAuthenticationService(factory).SignInAsync("legacyplayer", "secret-123", token);
        var profile = await new SqlitePlayerRepository(factory).GetAsync("42", token);
        await using UserDataDbContext context = factory.CreateDbContext();
        Assert.True(login.IsSuccess);
        Assert.Equal("Legacy Hero", profile!.Nickname);
        Assert.Equal(500, profile.Money);
        Assert.Equal(1, await context.Accounts.CountAsync(token));
        Assert.Equal(1, await context.DataMigrations.CountAsync(token));
        Assert.True(File.Exists(Path.Combine(paths.ApplicationDirectory, "id.txt")));
    }

    [Fact]
    public async Task Initializer_MigratesVersionedJsonAccountAndProfile()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        Directory.CreateDirectory(paths.PlayersDirectory);
        byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        byte[] hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            "secret-123", salt, 210_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        await File.WriteAllTextAsync(Path.Combine(paths.UserDataDirectory, "accounts.json"),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                accounts = new[] { new { login = "JsonPlayer", playerId = "7", salt = Convert.ToBase64String(salt), hash = Convert.ToBase64String(hash), iterations = 210_000 } }
            }, jsonOptions), token);
        await File.WriteAllTextAsync(Path.Combine(paths.PlayersDirectory, "7.json"),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                profile = new TheGame.Core.Players.PlayerProfile("7", "Json Hero", 2, 10, 300, "101", ["101"], "201", ["201"])
            }, jsonOptions), token);
        var factory = new UserContextFactory(paths.UserDatabasePath);
        await new UserDatabaseInitializer(paths, factory, new LegacyUserDataMigrator(paths, factory)).InitializeAsync(token);

        var login = await new SqliteAuthenticationService(factory).SignInAsync("jsonplayer", "secret-123", token);
        var profile = await new SqlitePlayerRepository(factory).GetAsync("7", token);

        Assert.True(login.IsSuccess);
        Assert.Equal("Json Hero", profile!.Nickname);
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
        Assert.Equal(context.Database.GetMigrations().Count(), await context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM __EFMigrationsHistory").SingleAsync(token));
        Assert.Single(Directory.EnumerateFiles(Path.Combine(paths.UserDataDirectory, "backups"), "*.db"));
    }

    [Fact]
    public async Task Initializer_RejectsCorruptedDatabaseWithoutOverwritingIt()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        IAppPaths paths = new AppPaths(_directory, Path.Combine(_directory, "data"));
        Directory.CreateDirectory(paths.UserDataDirectory);
        byte[] corrupted = [0x01, 0x02, 0x03, 0x04];
        await File.WriteAllBytesAsync(paths.UserDatabasePath, corrupted, token);
        var factory = new UserContextFactory(paths.UserDatabasePath);

        await Assert.ThrowsAsync<DataFormatException>(
            () => new UserDatabaseInitializer(paths, factory).InitializeAsync(token));

        Assert.Equal(corrupted, await File.ReadAllBytesAsync(paths.UserDatabasePath, token));
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
