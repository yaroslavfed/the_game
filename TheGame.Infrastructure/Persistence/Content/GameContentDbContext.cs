using Microsoft.EntityFrameworkCore;

namespace TheGame.Infrastructure.Persistence.Content;

public sealed class GameContentDbContext(DbContextOptions<GameContentDbContext> options) : DbContext(options)
{
    public DbSet<ContentInfoEntity> ContentInfo => Set<ContentInfoEntity>();
    public DbSet<ContentItemEntity> Items => Set<ContentItemEntity>();
    public DbSet<EnemyEntity> Enemies => Set<EnemyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentInfoEntity>(entity =>
        {
            entity.ToTable("content_info", table => table.HasCheckConstraint("ck_content_info_singleton", "id = 1"));
            entity.HasKey(value => value.Id);
            entity.Property(value => value.ContentVersion).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<ContentItemEntity>(entity =>
        {
            entity.ToTable("items", table =>
            {
                table.HasCheckConstraint("ck_items_kind", "kind IN (0, 1)");
                table.HasCheckConstraint("ck_items_power", "power >= 0");
                table.HasCheckConstraint("ck_items_price", "price >= 0");
                table.HasCheckConstraint("ck_items_rarity", "rarity >= 0");
            });
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).HasMaxLength(64);
            entity.Property(value => value.Name).HasMaxLength(200).IsRequired();
            entity.Property(value => value.Description).HasMaxLength(2000);
            entity.HasIndex(value => new { value.Kind, value.IsActive, value.SortOrder });
        });

        modelBuilder.Entity<EnemyEntity>(entity =>
        {
            entity.ToTable("enemies", table =>
            {
                table.HasCheckConstraint("ck_enemies_health", "health > 0");
                table.HasCheckConstraint("ck_enemies_damage", "damage >= 0");
                table.HasCheckConstraint("ck_enemies_protection", "protection >= 0");
                table.HasCheckConstraint("ck_enemies_reward", "reward >= 0");
            });
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).HasMaxLength(64);
            entity.Property(value => value.Rank).HasMaxLength(64).IsRequired();
            entity.Property(value => value.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(value => value.Rank).IsUnique();
            entity.HasIndex(value => new { value.IsActive, value.SortOrder });
        });
    }
}
