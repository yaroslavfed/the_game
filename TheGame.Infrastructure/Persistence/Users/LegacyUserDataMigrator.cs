using System.Security.Cryptography;
using System.Text.Json;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class LegacyUserDataMigrator(
    IAppPaths paths,
    IDbContextFactory<UserDataDbContext> contextFactory)
{
    private const string MigrationId = "legacy-user-storage-v1";
    private const int DefaultIterations = 210_000;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
            try
            {
                await using FileStream stream = File.OpenRead(jsonPath);
                using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
                JsonElement array = document.RootElement.ValueKind == JsonValueKind.Array
                    ? document.RootElement
                    : document.RootElement.GetProperty("accounts");
                if (array.ValueKind != JsonValueKind.Array)
                    throw new DataFormatException($"Legacy account document '{jsonPath}' does not contain an account array.");
                foreach (JsonElement item in array.EnumerateArray())
                {
                    string login = RequiredString(item, "login");
                    string playerId = RequiredString(item, "playerId");
                    byte[] salt = Convert.FromBase64String(RequiredString(item, "salt"));
                    byte[] hash = Convert.FromBase64String(RequiredString(item, "hash"));
                    int iterations = item.GetProperty("iterations").GetInt32();
                    if (iterations <= 0 || salt.Length == 0 || hash.Length == 0)
                        throw new DataFormatException($"Legacy account document '{jsonPath}' contains invalid credentials.");
                    var account = new ImportedAccount(
                        login, Normalize(login), playerId, salt, hash, iterations);
                    result[account.NormalizedLogin] = account;
                }
            }
            catch (DataFormatException)
            {
                throw;
            }
            catch (Exception exception) when (exception is JsonException or KeyNotFoundException or
                                               InvalidOperationException or FormatException)
            {
                throw new DataFormatException($"Legacy account document '{jsonPath}' is invalid.", exception);
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

    private async Task<PlayerProfile> LoadProfileAsync(string playerId, string login, CancellationToken token)
    {
        string jsonPath = Path.Combine(paths.PlayersDirectory, $"{playerId}.json");
        if (File.Exists(jsonPath))
        {
            try
            {
                await using FileStream stream = File.OpenRead(jsonPath);
                PlayerDocument? document = await JsonSerializer.DeserializeAsync<PlayerDocument>(stream, JsonOptions, token);
                if (document is null || document.SchemaVersion != 1 || document.Profile is null ||
                    document.Profile.Id != playerId)
                    throw new DataFormatException($"Legacy player document '{jsonPath}' is invalid.");
                return document.Profile;
            }
            catch (DataFormatException)
            {
                throw;
            }
            catch (JsonException exception)
            {
                throw new DataFormatException($"Legacy player document '{jsonPath}' is invalid.", exception);
            }
        }

        string textPath = Path.Combine(paths.LegacyUsersDirectory, $"{playerId}.txt");
        if (File.Exists(textPath))
        {
            string[] lines = await File.ReadAllLinesAsync(textPath, token);
            if (lines.Length < 8) throw new DataFormatException($"Legacy player document '{textPath}' is incomplete.");
            return new PlayerProfile(
                playerId, lines[0], ParseInt(lines[1], textPath), ParseInt(lines[2], textPath), ParseInt(lines[3], textPath),
                lines[4], SplitIds(lines[5]), lines[6], SplitIds(lines[7]));
        }

        return new PlayerProfile(playerId, login, 1, 0, 0, "10", ["10"], "20", ["20"]);
    }

    private static PlayerEntity Map(PlayerProfile profile)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return new PlayerEntity
        {
            Id = profile.Id, Nickname = profile.Nickname, Level = profile.Level, Experience = profile.Experience,
            Money = profile.Money, HighestWave = profile.HighestWave, CreatedAt = now, UpdatedAt = now,
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
    private static string RequiredString(JsonElement element, string propertyName)
    {
        string? value = element.GetProperty(propertyName).GetString();
        return string.IsNullOrWhiteSpace(value)
            ? throw new DataFormatException($"Legacy account field '{propertyName}' is required.")
            : value;
    }
    private static string Value(string line)
    {
        int separatorIndex = line.IndexOf(": ", StringComparison.Ordinal);
        if (separatorIndex < 1 || separatorIndex + 2 >= line.Length)
            throw new DataFormatException("Legacy account document contains an invalid field.");
        return line[(separatorIndex + 2)..];
    }
    private static byte[] Hash(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
    private static int ParseInt(string value, string path) => int.TryParse(
        value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
        ? result
        : throw new DataFormatException($"Legacy player document '{path}' contains an invalid number.");
    private static string[] SplitIds(string value) =>
        value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private sealed record ImportedAccount(string Login, string NormalizedLogin, string PlayerId, byte[] Salt, byte[] Hash, int Iterations);
    private sealed record PlayerDocument(int SchemaVersion, PlayerProfile? Profile);
}
