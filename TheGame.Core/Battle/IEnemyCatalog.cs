namespace TheGame.Core.Battle;

public interface IEnemyCatalog
{
    Task<EnemyDefinition?> GetAsync(string rank, CancellationToken cancellationToken = default);
}
