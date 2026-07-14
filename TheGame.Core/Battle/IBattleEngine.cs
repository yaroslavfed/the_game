namespace TheGame.Core.Battle;

public interface IBattleEngine
{
    BattleSession Attack(BattleSession session, string enemyId);

    BattleSession EnemyAttack(BattleSession session, string enemyId);

    BattleSession Heal(BattleSession session, double fractionOfMaxHealth);
}
