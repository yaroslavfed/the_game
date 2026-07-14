namespace the_game.Lifecycle;

public sealed record BattleTimingOptions(TimeSpan EnemyResponseDelay, TimeSpan HealCooldown)
{
    public static BattleTimingOptions Default { get; } = new(
        TimeSpan.FromMilliseconds(750),
        TimeSpan.FromSeconds(10));
}
