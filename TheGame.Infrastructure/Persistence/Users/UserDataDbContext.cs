using Microsoft.EntityFrameworkCore;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class UserDataDbContext(DbContextOptions<UserDataDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<PlayerItemEntity> PlayerItems => Set<PlayerItemEntity>();
    public DbSet<PlayerLoadoutEntity> PlayerLoadouts => Set<PlayerLoadoutEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.ToTable("accounts", table => table.HasCheckConstraint("ck_accounts_hash_iterations", "HashIterations > 0"));
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).HasMaxLength(64);
            entity.Property(value => value.Login).HasMaxLength(100).IsRequired();
            entity.Property(value => value.NormalizedLogin).HasMaxLength(100).IsRequired();
            entity.Property(value => value.PasswordHash).IsRequired();
            entity.Property(value => value.PasswordSalt).IsRequired();
            entity.HasIndex(value => value.NormalizedLogin).IsUnique();
        });

        modelBuilder.Entity<PlayerEntity>(entity =>
        {
            entity.ToTable("players", table =>
            {
                table.HasCheckConstraint("ck_players_level", "level >= 1");
                table.HasCheckConstraint("ck_players_experience", "experience >= 0");
                table.HasCheckConstraint("ck_players_money", "money >= 0");
                table.HasCheckConstraint("ck_players_revision", "revision >= 0");
            });
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).HasMaxLength(64);
            entity.Property(value => value.Nickname).HasMaxLength(100).IsRequired();
            entity.HasOne(value => value.Account).WithOne(value => value.Player)
                .HasForeignKey<PlayerEntity>(value => value.AccountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerItemEntity>(entity =>
        {
            entity.ToTable("player_items", table => table.HasCheckConstraint("ck_player_items_kind", "Kind IN (0, 1)"));
            entity.HasKey(value => new { value.PlayerId, value.ItemId });
            entity.Property(value => value.ItemId).HasMaxLength(64);
            entity.Property(value => value.Source).HasMaxLength(64);
            entity.HasOne(value => value.Player).WithMany(value => value.Items)
                .HasForeignKey(value => value.PlayerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerLoadoutEntity>(entity =>
        {
            entity.ToTable("player_loadouts");
            entity.HasKey(value => value.PlayerId);
            entity.Property(value => value.WeaponItemId).HasMaxLength(64);
            entity.Property(value => value.ArmorItemId).HasMaxLength(64);
            entity.HasOne(value => value.Player).WithOne(value => value.Loadout)
                .HasForeignKey<PlayerLoadoutEntity>(value => value.PlayerId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
