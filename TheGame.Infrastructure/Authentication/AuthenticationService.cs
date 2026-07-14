using System.Security.Cryptography;
using System.Text.Json;
using TheGame.Core.Authentication;
using TheGame.Core.Players;
using TheGame.Core.Storage;
using TheGame.Infrastructure.Storage;

namespace TheGame.Infrastructure.Authentication;

public sealed class AuthenticationService(IAppPaths paths, IPlayerRepository players) : IAuthenticationService
{
    private const int Iterations = 210_000;
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string StorePath => Path.Combine(paths.UserDataDirectory, "accounts.json");

    public async Task<AuthenticationResult> SignInAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        login = login.Trim();
        await _gate.WaitAsync(cancellationToken);
        try
        {
            List<Account> accounts = await LoadAsync(cancellationToken);
            Account? account = accounts.FirstOrDefault(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase));
            if (account is not null)
            {
                return Verify(password, account) ? AuthenticationResult.Success(account.PlayerId) : AuthenticationResult.Failure("Неверный пароль");
            }

            LegacyAccount? legacy = await FindLegacyAsync(login, cancellationToken);
            if (legacy is null)
            {
                return AuthenticationResult.Failure("Пользователь не найден");
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(password),
                    System.Text.Encoding.UTF8.GetBytes(legacy.Password)))
            {
                return AuthenticationResult.Failure("Неверный пароль");
            }

            accounts.Add(CreateAccount(login, password, legacy.PlayerId));
            await SaveAsync(accounts, cancellationToken);
            return AuthenticationResult.Success(legacy.PlayerId);
        }
        finally { _gate.Release(); }
    }

    public async Task<AuthenticationResult> RegisterAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        login = login.Trim();
        if (login.Length < 3) return AuthenticationResult.Failure("Логин должен содержать минимум 3 символа");
        if (password.Length < 8) return AuthenticationResult.Failure("Пароль должен содержать минимум 8 символов");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            List<Account> accounts = await LoadAsync(cancellationToken);
            if (accounts.Any(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)) ||
                await FindLegacyAsync(login, cancellationToken) is not null)
            {
                return AuthenticationResult.Failure("Логин уже занят");
            }

            int modernMax = accounts.Select(x => int.TryParse(x.PlayerId, out int id) ? id : 0).DefaultIfEmpty().Max();
            int nextId = Math.Max(modernMax, await GetLegacyMaxIdAsync(cancellationToken)) + 1;
            string playerId = nextId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            accounts.Add(CreateAccount(login, password, playerId));
            await SaveAsync(accounts, cancellationToken);
            await players.SaveAsync(new PlayerProfile(playerId, login, 1, 0, 0, "10", ["10"], "20", ["20"]), cancellationToken);
            return AuthenticationResult.Success(playerId);
        }
        finally { _gate.Release(); }
    }

    private async Task<List<Account>> LoadAsync(CancellationToken token)
    {
        if (!File.Exists(StorePath)) return [];
        try
        {
            await using FileStream stream = File.OpenRead(StorePath);
            using JsonDocument json = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            if (json.RootElement.ValueKind == JsonValueKind.Array)
            {
                return json.RootElement.Deserialize<List<Account>>(JsonOptions) ?? [];
            }

            AccountStore? store = json.RootElement.Deserialize<AccountStore>(JsonOptions);
            if (store is null || store.SchemaVersion != CurrentSchemaVersion)
            {
                throw new DataFormatException(
                    $"Account store uses an unsupported schema version ({store?.SchemaVersion}).");
            }
            return store.Accounts ?? [];
        }
        catch (JsonException exception)
        {
            throw new DataFormatException("Account store contains invalid JSON.", exception);
        }
    }

    private Task SaveAsync(List<Account> accounts, CancellationToken token) =>
        AtomicJsonFile.WriteAsync(
            StorePath,
            new AccountStore(CurrentSchemaVersion, accounts),
            JsonOptions,
            token);

    private async Task<LegacyAccount?> FindLegacyAsync(string login, CancellationToken token)
    {
        string file = Path.Combine(paths.ApplicationDirectory, "id.txt");
        if (!File.Exists(file)) return null;
        string[] lines = await File.ReadAllLinesAsync(file, token);
        for (int i = 0; i + 2 < lines.Length; i += 3)
        {
            string legacyLogin = Value(lines[i]);
            if (string.Equals(legacyLogin, login, StringComparison.OrdinalIgnoreCase))
                return new LegacyAccount(legacyLogin, Value(lines[i + 1]), Value(lines[i + 2]));
        }
        return null;
    }

    private async Task<int> GetLegacyMaxIdAsync(CancellationToken token)
    {
        string file = Path.Combine(paths.ApplicationDirectory, "id.txt");
        if (!File.Exists(file)) return 0;
        string[] lines = await File.ReadAllLinesAsync(file, token);
        return lines.Where(line => line.StartsWith("ID: ", StringComparison.Ordinal))
            .Select(line => int.TryParse(Value(line), out int id) ? id : 0)
            .DefaultIfEmpty().Max();
    }

    private static Account CreateAccount(string login, string password, string playerId)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return new Account(login, playerId, Convert.ToBase64String(salt), Convert.ToBase64String(hash), Iterations);
    }

    private static bool Verify(string password, Account account)
    {
        byte[] salt = Convert.FromBase64String(account.Salt);
        byte[] expected = Convert.FromBase64String(account.Hash);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, account.Iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static string Value(string line) => line[(line.IndexOf(": ", StringComparison.Ordinal) + 2)..];
    private sealed record Account(string Login, string PlayerId, string Salt, string Hash, int Iterations);
    private sealed record AccountStore(int SchemaVersion, List<Account> Accounts);
    private sealed record LegacyAccount(string Login, string Password, string PlayerId);
}
