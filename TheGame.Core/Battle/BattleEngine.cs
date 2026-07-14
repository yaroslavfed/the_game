namespace TheGame.Core.Battle;

public sealed class BattleEngine : IBattleEngine
{
    public BattleSession Attack(BattleSession session, string enemyId)
    {
        EnsureInProgress(session);
        int index = FindLivingEnemy(session, enemyId);
        BattleEnemy enemy = session.Enemies[index];
        double health = Math.Max(0, enemy.Health - ApplyProtection(session.Hero.Damage, enemy.Protection));
        BattleEnemy updatedEnemy = enemy with { Health = health };
        BattleEnemy[] enemies = [.. session.Enemies];
        enemies[index] = updatedEnemy;
        int reward = session.Reward + (health <= 0 ? enemy.Reward : 0);
        BattleStatus status = enemies.All(candidate => !candidate.IsAlive)
            ? BattleStatus.Victory
            : BattleStatus.InProgress;

        return session with { Enemies = enemies, Reward = reward, Status = status };
    }

    public BattleSession EnemyAttack(BattleSession session, string enemyId)
    {
        EnsureInProgress(session);
        BattleEnemy enemy = session.Enemies[FindLivingEnemy(session, enemyId)];
        double health = Math.Max(0, session.Hero.Health - ApplyProtection(enemy.Damage, session.Hero.Protection));
        BattleStatus status = health <= 0 ? BattleStatus.Defeat : BattleStatus.InProgress;
        return session with { Hero = session.Hero with { Health = health }, Status = status };
    }

    public BattleSession Heal(BattleSession session, double fractionOfMaxHealth)
    {
        EnsureInProgress(session);
        if (fractionOfMaxHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(fractionOfMaxHealth));

        double health = Math.Min(
            session.Hero.MaxHealth,
            session.Hero.Health + (session.Hero.MaxHealth * fractionOfMaxHealth));
        return session with { Hero = session.Hero with { Health = health } };
    }

    private static double ApplyProtection(double damage, double protection) =>
        Math.Max(0, damage) * (1 - Math.Clamp(protection, 0, 100) / 100);

    private static int FindLivingEnemy(BattleSession session, string enemyId)
    {
        int index = session.Enemies.ToList().FindIndex(enemy => enemy.Id == enemyId);
        if (index < 0)
            throw new KeyNotFoundException($"Enemy '{enemyId}' was not found.");
        if (!session.Enemies[index].IsAlive)
            throw new InvalidOperationException($"Enemy '{enemyId}' is already defeated.");
        return index;
    }

    private static void EnsureInProgress(BattleSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.Status != BattleStatus.InProgress)
            throw new InvalidOperationException("The battle has already finished.");
    }
}
