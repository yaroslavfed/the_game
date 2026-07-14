namespace TheGame.Core.Battle;

public sealed record EnemyDefinition(
    string Rank,
    string Name,
    double Health,
    double Damage,
    double Protection,
    int Reward);
