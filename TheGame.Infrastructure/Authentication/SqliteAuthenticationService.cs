using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Authentication;
using TheGame.Core.Inventory;
using TheGame.Infrastructure.Persistence.Users;

namespace TheGame.Infrastructure.Authentication;

public sealed class SqliteAuthenticationService(IDbContextFactory<UserDataDbContext> contextFactory) : IAuthenticationService
{
    private const int Iterations = 210_000;

    public async Task<AuthenticationResult> SignInAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        string normalized = Normalize(login);
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        AccountEntity? account = await context.Accounts.Include(value => value.Player)
            .SingleOrDefaultAsync(value => value.NormalizedLogin == normalized, cancellationToken);
        if (account is null) return AuthenticationResult.Failure("Пользователь не найден");
        if (!Verify(password, account)) return AuthenticationResult.Failure("Неверный пароль");
        account.LastLoginAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return AuthenticationResult.Success(account.Player?.Id ?? account.Id);
    }

    public async Task<AuthenticationResult> RegisterAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        login = login.Trim();
        if (login.Length < 3) return AuthenticationResult.Failure("Логин должен содержать минимум 3 символа");
        if (password.Length < 8) return AuthenticationResult.Failure("Пароль должен содержать минимум 8 символов");
        string normalized = Normalize(login);
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.Accounts.AnyAsync(value => value.NormalizedLogin == normalized, cancellationToken))
            return AuthenticationResult.Failure("Логин уже занят");

        string id = Guid.NewGuid().ToString("N");
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        var account = new AccountEntity
        {
            Id = id, Login = login, NormalizedLogin = normalized, PasswordSalt = salt,
            PasswordHash = Hash(password, salt, Iterations), HashIterations = Iterations, CreatedAt = DateTimeOffset.UtcNow,
            Player = new PlayerEntity
            {
                Id = id, Nickname = login, Level = 1, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow,
                Items =
                [
                    new PlayerItemEntity { ItemId = "10", Kind = InventoryItemKind.Weapon, AcquiredAt = DateTimeOffset.UtcNow, Source = "registration" },
                    new PlayerItemEntity { ItemId = "20", Kind = InventoryItemKind.Armor, AcquiredAt = DateTimeOffset.UtcNow, Source = "registration" }
                ],
                Loadout = new PlayerLoadoutEntity { WeaponItemId = "10", ArmorItemId = "20" }
            }
        };
        context.Accounts.Add(account);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return AuthenticationResult.Failure("Логин уже занят");
        }
        return AuthenticationResult.Success(id);
    }

    private static string Normalize(string login) => login.Trim().ToUpperInvariant();
    private static byte[] Hash(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
    private static bool Verify(string password, AccountEntity account) =>
        CryptographicOperations.FixedTimeEquals(Hash(password, account.PasswordSalt, account.HashIterations), account.PasswordHash);
}
