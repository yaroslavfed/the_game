namespace TheGame.Infrastructure.Persistence.Users;

public sealed class AccountEntity
{
    public string Id { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string NormalizedLogin { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
    public int HashIterations { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public PlayerEntity? Player { get; set; }
}

public sealed class PlayerEntity
{
    public string Id { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Experience { get; set; }
    public int Money { get; set; }
    public int Revision { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public AccountEntity Account { get; set; } = null!;
    public PlayerLoadoutEntity? Loadout { get; set; }
    public ICollection<PlayerItemEntity> Items { get; set; } = [];
}

public sealed class PlayerItemEntity
{
    public string PlayerId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public DateTimeOffset AcquiredAt { get; set; }
    public string? Source { get; set; }
    public PlayerEntity Player { get; set; } = null!;
}

public sealed class PlayerLoadoutEntity
{
    public string PlayerId { get; set; } = string.Empty;
    public string? WeaponItemId { get; set; }
    public string? ArmorItemId { get; set; }
    public PlayerEntity Player { get; set; } = null!;
}
