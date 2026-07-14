namespace TheGame.Core.Battle;

public sealed record BattleSession(
    BattleHero Hero,
    IReadOnlyList<BattleEnemy> Enemies,
    int Reward,
    BattleStatus Status)
{
    public static BattleSession Start(BattleHero hero, IReadOnlyList<BattleEnemy> enemies)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(enemies);
        if (hero.MaxHealth <= 0 || hero.Health <= 0 || hero.Health > hero.MaxHealth)
            throw new ArgumentOutOfRangeException(nameof(hero), "Hero health must be within the maximum health range.");
        if (hero.Damage < 0)
            throw new ArgumentOutOfRangeException(nameof(hero), "Hero damage cannot be negative.");
        if (enemies.Count == 0 || enemies.Any(enemy => !enemy.IsAlive || enemy.MaxHealth <= 0))
            throw new ArgumentException("A battle requires at least one living enemy.", nameof(enemies));
        if (enemies.Select(enemy => enemy.Id).Distinct(StringComparer.Ordinal).Count() != enemies.Count)
            throw new ArgumentException("Enemy identifiers must be unique.", nameof(enemies));

        return new BattleSession(hero, [.. enemies], 0, BattleStatus.InProgress);
    }
}
