using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Players;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class LegacyUserDataMigrator(
    IAppPaths paths,
    IDbContextFactory<UserDataDbContext> contextFactory)
{
    private const string MigrationId = "legacy-user-storage-v1";
    private const int DefaultIterations = 210_000;
    private readonly JsonPlayerRepository _jsonPlayers = new(paths);
    private readonly LegacyPlayerRepository _legacyPlayers = new(paths);

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using UserDataDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.DataMigrations.AnyAsync(value => value.Id == MigrationId, cancellationToken)) return;

        IReadOnlyList<ImportedAccount> accounts = await LoadAccountsAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        int imported = 0;
        foreach (ImportedAccount source in accounts)
        {
            if (await context.Accounts.AnyAsync(value => value.NormalizedLogin == source.NormalizedLogin, cancellationToken))
                continue;

            PlayerProfile profile = await LoadProfileAsync(source.PlayerId, source.Login, cancellationToken);
            var account = new AccountEntity
            {
                Id = $"legacy-{source.PlayerId}", Login = source.Login, NormalizedLogin = source.NormalizedLogin,
                PasswordHash = source.Hash, PasswordSalt = source.Salt, HashIterations = source.Iterations,
                CreatedAt = DateTimeOffset.UtcNow,
                Player = Map(profile)
            };
            context.Accounts.Add(account);
            imported++;
        }

        context.DataMigrations.Add(new DataMigrationEntity
        {
            Id = MigrationId, CompletedAt = DateTimeOffset.UtcNow, Details = $"Imported accounts: {imported}"
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ImportedAccount>> LoadAccountsAsync(CancellationToken token)
    {
        var result = new Dictionary<string, ImportedAccount>(StringComparer.Ordinal);
        string jsonPath = Path.Combine(paths.UserDataDirectory, "accounts.json");
        if (File.Exists(jsonPath))
        {
            await using FileStream stream = File.OpenRead(jsonPath);
            using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            JsonElement array = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement
                : document.RootElement.GetProperty("accounts");
            foreach (JsonElement item in array.EnumerateArray())
            {
                string login = item.GetProperty("login").GetString()!;
                var account = new ImportedAccount(
                    login, Normalize(login), item.GetProperty("playerId").GetString()!,
                    Convert.FromBase64String(item.GetProperty("salt").GetString()!),
                    Convert.FromBase64String(item.GetProperty("hash").GetString()!),
                    item.GetProperty("iterations").GetInt32());
                result[account.NormalizedLogin] = account;
            }
        }

        string legacyPath = Path.Combine(paths.ApplicationDirectory, "id.txt");
        if (File.Exists(legacyPath))
        {
            string[] lines = await File.ReadAllLinesAsync(legacyPath, token);
            for (int index = 0; index + 2 < lines.Length; index += 3)
            {
                string login = Value(lines[index]);
                string normalized = Normalize(login);
                if (result.ContainsKey(normalized)) continue;
                string password = Value(lines[index + 1]);
                byte[] salt = RandomNumberGenerator.GetBytes(16);
                result[normalized] = new ImportedAccount(
                    login, normalized, Value(lines[index + 2]), salt,
                    Hash(password, salt, DefaultIterations), DefaultIterations);
            }
        }
        return result.Values.ToArray();
    }

    private async Task<PlayerProfile> LoadProfileAsync(string playerId, string login, CancellationToken token) =>
        await _jsonPlayers.GetAsync(playerId, token)
        ?? await _legacyPlayers.GetAsync(playerId, token)
        ?? new PlayerProfile(playerId, login, 1, 0, 0, "10", ["10"], "20", ["20"]);

    private static PlayerEntity Map(PlayerProfile profile)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return new PlayerEntity
        {
            Id = profile.Id, Nickname = profile.Nickname, Level = profile.Level, Experience = profile.Experience,
            Money = profile.Money, CreatedAt = now, UpdatedAt = now,
            Items = profile.OwnedWeaponIds.Select(id => new PlayerItemEntity
                { ItemId = id, Kind = InventoryItemKind.Weapon, AcquiredAt = now, Source = "legacy-migration" })
                .Concat(profile.OwnedArmorIds.Select(id => new PlayerItemEntity
                    { ItemId = id, Kind = InventoryItemKind.Armor, AcquiredAt = now, Source = "legacy-migration" }))
                .GroupBy(item => item.ItemId).Select(group => group.First()).ToList(),
            Loadout = new PlayerLoadoutEntity
                { WeaponItemId = profile.EquippedWeaponId, ArmorItemId = profile.EquippedArmorId }
        };
    }

    private static string Normalize(string login) => login.Trim().ToUpperInvariant();
    private static string Value(string line) => line[(line.IndexOf(": ", StringComparison.Ordinal) + 2)..];
    private static byte[] Hash(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
    private sealed record ImportedAccount(string Login, string NormalizedLogin, string PlayerId, byte[] Salt, byte[] Hash, int Iterations);
}
