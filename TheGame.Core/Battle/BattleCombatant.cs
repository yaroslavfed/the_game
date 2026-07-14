namespace TheGame.Core.Battle;

public sealed record BattleHero(
    double Health,
    double MaxHealth,
    double Damage,
    double Protection);

public sealed record BattleEnemy(
    string Id,
    string Rank,
    string Name,
    double Health,
    double MaxHealth,
    double Damage,
    double Protection,
    int Reward)
{
    public bool IsAlive => Health > 0;

    public static BattleEnemy Create(string id, EnemyDefinition definition) =>
        new(
            id,
            definition.Rank,
            definition.Name,
            definition.Health,
            definition.Health,
            definition.Damage,
            definition.Protection,
            definition.Reward);
}
