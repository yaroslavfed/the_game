using TheGame.Core.Battle;
using Xunit;

namespace the_game.Tests.Battle;

public sealed class BattleEngineTests
{
    private readonly BattleEngine _engine = new();

    [Fact]
    public void Attack_AppliesProtectionAndDoesNotMutatePreviousSession()
    {
        BattleSession initial = CreateSession(heroDamage: 40, enemyHealth: 100, enemyProtection: 25);

        BattleSession result = _engine.Attack(initial, "enemy-1");

        Assert.Equal(70, result.Enemies[0].Health);
        Assert.Equal(100, initial.Enemies[0].Health);
        Assert.Equal(BattleStatus.InProgress, result.Status);
    }

    [Fact]
    public void Attack_LastEnemyAwardsRewardAndCompletesBattle()
    {
        BattleSession initial = CreateSession(heroDamage: 200, enemyHealth: 100, reward: 35);

        BattleSession result = _engine.Attack(initial, "enemy-1");

        Assert.Equal(0, result.Enemies[0].Health);
        Assert.Equal(35, result.Reward);
        Assert.Equal(BattleStatus.Victory, result.Status);
    }

    [Fact]
    public void EnemyAttack_ClampsHealthAndMarksDefeat()
    {
        BattleSession initial = CreateSession(heroHealth: 20, enemyDamage: 100);

        BattleSession result = _engine.EnemyAttack(initial, "enemy-1");

        Assert.Equal(0, result.Hero.Health);
        Assert.Equal(BattleStatus.Defeat, result.Status);
    }

    [Fact]
    public void Heal_CannotExceedMaximumHealth()
    {
        BattleSession initial = CreateSession(heroHealth: 80);

        BattleSession result = _engine.Heal(initial, 0.5);

        Assert.Equal(100, result.Hero.Health);
    }

    [Fact]
    public void FinishedBattle_RejectsFurtherActions()
    {
        BattleSession victory = _engine.Attack(
            CreateSession(heroDamage: 200, enemyHealth: 100),
            "enemy-1");

        Assert.Throws<InvalidOperationException>(() => _engine.Heal(victory, 0.5));
    }

    private static BattleSession CreateSession(
        double heroHealth = 100,
        double heroDamage = 20,
        double enemyHealth = 100,
        double enemyDamage = 10,
        double enemyProtection = 0,
        int reward = 10)
    {
        var hero = new BattleHero(heroHealth, 100, heroDamage, 0);
        var enemy = new BattleEnemy(
            "enemy-1", "0", "Enemy", enemyHealth, enemyHealth, enemyDamage, enemyProtection, reward);
        return BattleSession.Start(hero, [enemy]);
    }
}
