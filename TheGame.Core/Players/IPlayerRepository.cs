namespace TheGame.Core.Players;

public interface IPlayerRepository
{
    Task<PlayerProfile?> GetAsync(string playerId, CancellationToken cancellationToken = default);

    Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default);
}
